using EY.Energy.Application.DTO.Chat;
using EY.Energy.Application.Services.Users;
using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EY.Energy.API.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connections;
        private readonly IMongoCollection<ChatRoomInvitation> _chatRooms;
        private readonly IMongoCollection<User> _users;
        private readonly UserServices userServices;

        public ChatHub(IDictionary<string, UserRoomConnection> connections, MongoDBContext context, UserServices userServices)
        {
            _connections = connections;
            _chatRooms = context.Invitations;
            _users = context.Users;
            this.userServices = userServices;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            _connections[Context.ConnectionId] = new UserRoomConnection
            {
                User = userName,
                Room = string.Empty
            };

            var userRooms = await _chatRooms.Find(r => r.Users.Contains(userName)).ToListAsync();
            var roomNames = userRooms.Select(r => r.RoomName).ToList();
            await Clients.Caller.SendAsync("UserRooms", roomNames);

            if (Context.User!.IsInRole("Manager"))
            {
                // Send all consultants to the manager
                var consultants = await GetConsultants();
                await Clients.Caller.SendAsync("AllUsers", consultants);
            }

            await base.OnConnectedAsync();
        }

        public async Task<List<UserStatus>> GetConsultants()
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            var ConsultantAndCostumerUsers = await userServices.GetUsersByRole(Role.Consultant , Role.Customer) ;
            var userStatuses = ConsultantAndCostumerUsers
                .Where(u => u.Username != userName)
                .Select(u => new UserStatus
                {
                    Username = u.Username,
                    IsOnline = _connections.Values.Any(c => c.User == u.Username)
                }).ToList();
            return userStatuses;
        }

        public async Task<IEnumerable<string>> GetRooms()
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            var rooms = await _chatRooms.Find(r => r.Users.Contains(userName)).ToListAsync();
            return rooms.Select(r => r.RoomName);
        }

        public async Task<IEnumerable<string>> GetUsersInRoom(string roomName)
        {
            var room = await _chatRooms.Find(r => r.RoomName == roomName && r.Users.Contains(Context.User!.Identity!.Name!)).FirstOrDefaultAsync();
            return room?.Users ?? new List<string>();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesInRoom(string roomName)
        {
            var room = await _chatRooms.Find(r => r.RoomName == roomName && r.Users.Contains(Context.User!.Identity!.Name!)).FirstOrDefaultAsync();
            return room?.Messages ?? new List<ChatMessage>();
        }

       

        public IEnumerable<string> GetConnectedUsers()
        {
            return _connections.Values.Select(c => c.User).Distinct();
        }

        public async Task JoinRoom(string roomName)
        {
            var user = _connections[Context.ConnectionId];
            if (user == null) return;

            var chatRoom = await _chatRooms.Find(r => r.RoomName == roomName).FirstOrDefaultAsync();
            if (chatRoom == null)
            {
                Console.WriteLine($"Room {roomName} does not exist.");
                return;
            }

            if (!chatRoom.Users.Contains(user.User))
            {
                Console.WriteLine($"User {user.User} is not authorized to join room {roomName}.");
                await Clients.Client(Context.ConnectionId).SendAsync("NotAuthorized");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            user.Room = roomName;

            var messages = chatRoom.Messages;
            await Clients.Client(Context.ConnectionId).SendAsync("LoadMessages", messages);
        }

        public async Task AddUserToRoom(string username, string roomName)
        {
            var chatRoom = await _chatRooms.Find(r => r.RoomName == roomName).FirstOrDefaultAsync();
            if (chatRoom == null)
            {
                Console.WriteLine($"Room {roomName} does not exist.");
                return;
            }

            if (!chatRoom.Users.Contains(username))
            {
                var updateResult = await _chatRooms.UpdateOneAsync(
                    Builders<ChatRoomInvitation>.Filter.Eq(r => r.RoomName, roomName),
                    Builders<ChatRoomInvitation>.Update.AddToSet(r => r.Users, username)
                );

                if (updateResult.ModifiedCount > 0)
                {
                    Console.WriteLine($"User {username} successfully added to room {roomName}");

                    var connectionId = _connections.FirstOrDefault(c => c.Value.User == username).Key;
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("OpenRoom", roomName);
                    }

                    await Clients.Caller.SendAsync("UserAddedToRoom", username, roomName);
                }
                else
                {
                    Console.WriteLine($"Failed to add user {username} to room {roomName}");
                }
            }
            else
            {
                Console.WriteLine($"User {username} already in room {roomName}");
            }
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserRoomConnection? userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room).SendAsync("ReceiveMessage", userRoomConnection.User, message, DateTime.Now);

                var chatMessage = new ChatMessage
                {
                    User = userRoomConnection.User,
                    Message = message,
                    Timestamp = DateTime.Now
                };

                await _chatRooms.UpdateOneAsync(
                    Builders<ChatRoomInvitation>.Filter.Eq(r => r.RoomName, userRoomConnection.Room),
                    Builders<ChatRoomInvitation>.Update.Push(r => r.Messages, chatMessage)
                );
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserRoomConnection? userRoomConnection))
            {
                _connections.Remove(Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task CreateRoom(string roomName)
        {
            var user = _connections[Context.ConnectionId];
            if (user == null) return;

            var chatRoom = new ChatRoomInvitation { RoomName = roomName };
            chatRoom.Users.Add(user.User);
            await _chatRooms.InsertOneAsync(chatRoom);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            user.Room = roomName;

        }
    }
}



