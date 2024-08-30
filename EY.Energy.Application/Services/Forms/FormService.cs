using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EY.Energy.Application.Services.Forms
{
    public class FormService
    {
        private readonly IMongoCollection<Form> _forms;
        private readonly IMongoCollection<Question> _questions;
        private readonly IMongoCollection<Option> _options;

        public FormService(MongoDBContext database)
        {
            _forms = database.Forms;
            _questions = database.Questions;
            _options = database.Options;
        }


        public List<Form> GetForms()
        {
            try
            {
                return _forms.Find(form => true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Get Form By Id 
        public Form GetForm(string formId)
        {
            try
            {
                return _forms.Find<Form>(f => f.FormId == formId.ToString()).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public string GetFormTitleByFormId(string formId)
        {
            try
            {
                var form = _forms.Find<Form>(f => f.FormId == formId).FirstOrDefault();
                return form!.Title;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Create Form 
        public Form CreateForm(Form form)
        {
            try
            {
                _forms.InsertOne(form);
                return form;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateFormTitleAsync(string formId, string newTitle)
        {
            try
            {
                var filter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());
                var formUpdate = Builders<Form>.Update.Set(f => f.Title, newTitle);
                await _forms.UpdateOneAsync(filter, formUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }


        public async Task DeleteFormAsync(ObjectId formId)
        {
            try
            {
                var filter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());
                await _forms.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public List<Question> GetQuestions()
        {
            try
            {
                return _questions.Find(question => true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
     
        // Get Question By Id 
        public async Task<Question> GetQuestionByIdAsync(string questionId)
        {
            try
            {
                return await _questions.Find<Question>(q => q.QuestionId == questionId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        // Set Question To Form 
        public async Task<Question> AddQuestionAsync(ObjectId formId, Question question)
        {
            try
            {
                var filter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());
                question.QuestionId = ObjectId.GenerateNewId().ToString();

                // Add the question to the Questions collection
                await _questions.InsertOneAsync(question);

                var update = Builders<Form>.Update.Push(f => f.Questions, question);
                await _forms.UpdateOneAsync(filter, update);

                return question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateQuestionTitleAsync(string questionId, string newTitle)
        {
            try
            {
                var filter = Builders<Question>.Filter.Eq(q => q.QuestionId, questionId);
                var update = Builders<Question>.Update.Set(q => q.Text, newTitle);
                await _questions.UpdateOneAsync(filter, update);
                var formFilter = Builders<Form>.Filter.Eq("Questions.QuestionId", questionId);
                var formUpdate = Builders<Form>.Update.Set("Questions.$.Text", newTitle);
                await _forms.UpdateOneAsync(formFilter, formUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteQuestionAsync(string questionId)
        {
            try
            {
                var filter = Builders<Question>.Filter.Eq(q => q.QuestionId, questionId);
                await _questions.DeleteOneAsync(filter);

                var formFilter = Builders<Form>.Filter.Eq("Questions.QuestionId", questionId);
                var update = Builders<Form>.Update.PullFilter(f => f.Questions, q => q.QuestionId == questionId);
                await _forms.UpdateOneAsync(formFilter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


      
        //Set Option to Question
        public async Task<Option> AddOptionAsync(ObjectId formId, ObjectId questionId, Option option)
        {
            try {
            option.OptionId = ObjectId.GenerateNewId().ToString();

            // Add the option to the Options collection
            await _options.InsertOneAsync(option);

            var filter = Builders<Form>.Filter.And(
                Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString()),
                Builders<Form>.Filter.Eq("Questions.QuestionId", questionId.ToString())
            );

            var update = Builders<Form>.Update.Push("Questions.$.Options", option);
            await _forms.UpdateOneAsync(filter, update);

            return option;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task UpdateOptionTitleAsync(ObjectId formId, ObjectId questionId, ObjectId optionId, string newTitle)
        {
            try
            {
                var optionFilter = Builders<Option>.Filter.Eq(o => o.OptionId, optionId.ToString());
                var optionUpdate = Builders<Option>.Update.Set(o => o.Text, newTitle);
                await _options.UpdateOneAsync(optionFilter, optionUpdate);

                var formFilter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());
                var form = await _forms.Find(formFilter).FirstOrDefaultAsync();

                if (form != null)
                {
                    var question = form.Questions.FirstOrDefault(q => q.QuestionId == questionId.ToString());
                    if (question != null)
                    {
                        var option = question.Options!.FirstOrDefault(o => o.OptionId == optionId.ToString());
                        if (option != null)
                        {
                            option.Text = newTitle;

                            var updateResult = await _forms.ReplaceOneAsync(formFilter, form);
                            if (updateResult.ModifiedCount == 0)
                            {
                                throw new Exception("Failed to update document.");
                            }
                        }
                        else
                        {
                            throw new Exception("Option not found.");
                        }
                    }
                    else
                    {
                        throw new Exception("Question not found.");
                    }
                }
                else
                {
                    throw new Exception("Form not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteOptionAsync(string optionId)
        {
            try
            {
                var filter = Builders<Option>.Filter.Eq(o => o.OptionId, optionId);
                await _options.DeleteOneAsync(filter);

                var formFilter = Builders<Form>.Filter.Eq("Questions.Options.OptionId", optionId);
                var update = Builders<Form>.Update.PullFilter("Questions.$[].Options", Builders<Option>.Filter.Eq(o => o.OptionId, optionId));
                await _forms.UpdateOneAsync(formFilter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        // Set subquestion to Option
        public async Task<Question> SetSubQuestionAsync(ObjectId formId, ObjectId questionId, ObjectId optionId, Question subQuestion)
        {
            try
            {
                subQuestion.QuestionId = ObjectId.GenerateNewId().ToString();

                // Ajouter la sous-question à la collection des questions
                await _questions.InsertOneAsync(subQuestion);

                // Filtre pour trouver le formulaire par ID
                var formFilter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());

                // Charger le document entier
                var form = await _forms.Find(formFilter).FirstOrDefaultAsync();
                if (form == null) throw new Exception("Form not found.");

                // Trouver la question et l'option et mettre à jour en mémoire
                var question = form.Questions.FirstOrDefault(q => q.QuestionId == questionId.ToString());
                if (question == null) throw new Exception("Question not found.");

                var option = question.Options?.FirstOrDefault(o => o.OptionId == optionId.ToString());
                if (option == null) throw new Exception("Option not found.");

                option.SubQuestions!.Add(subQuestion);

                // Réécrire le document entier
                var replaceResult = await _forms.ReplaceOneAsync(formFilter, form);
                if (replaceResult.ModifiedCount == 0) throw new Exception("Failed to update document.");

                return subQuestion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        //Ajouter option to subquestion
        public async Task AddOptionToSubQuestionAsync(ObjectId formId, ObjectId questionId, ObjectId subQuestionId, Option option)
        {
            try
            {
                option.OptionId = ObjectId.GenerateNewId().ToString();
                await _options.InsertOneAsync(option);

                var formFilter = Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString());

                var form = await _forms.Find(formFilter).FirstOrDefaultAsync();
                if (form == null)
                {
                    throw new Exception("Formulaire non trouvé.");
                }

                var question = form.Questions.FirstOrDefault(q => q.QuestionId == questionId.ToString());
                if (question == null)
                {
                    throw new Exception($"Question avec ID {questionId} non trouvée dans le formulaire avec ID {formId}.");
                }

                Option? optionTrouvée = null;
                Question? sousQuestionTrouvée = null;

                foreach (var opt in question.Options!)
                {
                    sousQuestionTrouvée = opt.SubQuestions?.FirstOrDefault(sq => sq.QuestionId == subQuestionId.ToString());
                    if (sousQuestionTrouvée != null)
                    {
                        optionTrouvée = opt;
                        break;
                    }
                }

                if (sousQuestionTrouvée == null)
                {
                    throw new Exception($"Sous-question avec ID {subQuestionId} non trouvée dans une option de la question avec ID {questionId}.");
                }

                sousQuestionTrouvée.Options!.Add(option);

                var update = Builders<Form>.Update.Set(f => f.Questions, form.Questions);
                var replaceResult = await _forms.UpdateOneAsync(formFilter, update);
                if (replaceResult.ModifiedCount == 0)
                {
                    throw new Exception("Échec de la mise à jour du document.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        // Set Next Question 
        public async Task AutoSetNextQuestions(ObjectId formId)
        {
            try
            {
                var form = await _forms.Find(f => f.FormId == formId.ToString()).FirstOrDefaultAsync();
                if (form == null) return;

                for (int i = 0; i < form.Questions.Count - 1; i++)
                {
                    var currentQuestion = form.Questions[i];
                    var nextQuestion = form.Questions[i + 1];

                    if (currentQuestion.TypeQuestion == TypeQuestion.Text && currentQuestion.NextQuestionId == null)
                    {
                        var filter = Builders<Form>.Filter.And(
                            Builders<Form>.Filter.Eq(f => f.FormId, formId.ToString()),
                            Builders<Form>.Filter.Eq("Questions.QuestionId", currentQuestion.QuestionId)
                        );

                        var update = Builders<Form>.Update.Set("Questions.$.NextQuestionId", nextQuestion.QuestionId);
                        await _forms.UpdateOneAsync(filter, update);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

    }
}
