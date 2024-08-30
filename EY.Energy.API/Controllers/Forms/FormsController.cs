using EY.Energy.Application.Services.Forms;
using EY.Energy.Entity;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace EY.Energy.API.Controllers.Forms
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : ControllerBase
    {

        private readonly FormService _formService;

        public FormsController(FormService formService)
        {
            _formService = formService;
        }

        // GET: api/forms
        [HttpGet]
        public ActionResult<List<Form>> Get()
        {
            try
            {
                return _formService.GetForms();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: api/forms/{id}
        [HttpGet("{id:length(24)}", Name = "GetForm")]
        public ActionResult<Form> Get(string id)
        {
            try
            {
                var form = _formService.GetForm(new(id));

                if (form == null)
                {
                    return NotFound();
                }

                return form;
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/forms/NameForm/{formId}
        [HttpGet("NameForm/{formId}")]
        public IActionResult GetNameFormByFormId(string formId)
        {
            try
            {
                var formTitle = _formService.GetFormTitleByFormId(formId);

                if (formTitle == null)
                {
                    return NotFound();
                }

                return Ok(new { title = formTitle }); // Wrap the response in an object
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/forms
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult<Form> Create(Form form)
        {
            try
            {
                _formService.CreateForm(form);
                return CreatedAtRoute("GetForm", new { id = form.FormId.ToString() }, form);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("updateForm/{formId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateFormTitle(string formId, [FromBody] string newTitle)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId))
            {
                return BadRequest("Invalid form ID.");
            }

            try
            {
                await _formService.UpdateFormTitleAsync(formObjectId.ToString(), newTitle);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete form
        [HttpDelete("Form/{formId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteForm(string formId)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId))
            {
                return BadRequest("Invalid form ID.");
            }

            try
            {
                await _formService.DeleteFormAsync(formObjectId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("questions/{questionId}")]
        public async Task<ActionResult<Question>> GetQuestionById(string questionId)
        {
            try
            {
                var question = await _formService.GetQuestionByIdAsync(questionId);

                if (question == null)
                {
                    return NotFound();
                }

                return question;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("{formId}/questions")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddQuestion(string formId, [FromBody] Question question)
        {
            try
            {
                if (!ObjectId.TryParse(formId, out ObjectId objectId))
                {
                    return BadRequest("Invalid form ID.");
                }

                var savedQuestion = await _formService.AddQuestionAsync(objectId, question);
                return Ok(savedQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // Update Question
        [HttpPut("questions/{questionId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateQuestion(string questionId, [FromBody] string newTitle)
        {
            try
            {
                await _formService.UpdateQuestionTitleAsync(questionId, newTitle);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete Question
        [HttpDelete("questions/{questionId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteQuestion(string questionId)
        {
            try
            {
                await _formService.DeleteQuestionAsync(questionId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("{formId}/questions/{questionId}/options")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddOption(string formId, string questionId, [FromBody] Option option)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId) || !ObjectId.TryParse(questionId, out ObjectId questionObjectId))
            {
                return BadRequest("Invalid ID.");
            }
            try
            {
                var savedOption = await _formService.AddOptionAsync(formObjectId, questionObjectId, option);
                return Ok(savedOption);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{formId}/questions/{questionId}/options/{optionId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateOptionTitle(string formId, string questionId, string optionId, [FromBody] string newTitle)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId) ||
                !ObjectId.TryParse(questionId, out ObjectId questionObjectId) ||
                !ObjectId.TryParse(optionId, out ObjectId optionObjectId))
            {
                return BadRequest("Invalid ID.");
            }

            try
            {
                await _formService.UpdateOptionTitleAsync(formObjectId, questionObjectId, optionObjectId, newTitle);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("options/{optionId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteOption(string optionId)
        {
            try
            {
                await _formService.DeleteOptionAsync(optionId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpPost("{formId}/questions/{questionId}/subQuestions/{subQuestionId}/options")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddOptionToSubQuestion(string formId, string questionId, string subQuestionId, [FromBody] Option option)
        {
            try
            {
                if (option == null)
                {
                    return BadRequest("Option is null.");
                }

                var formObjectId = new ObjectId(formId);
                var questionObjectId = new ObjectId(questionId);
                var subQuestionObjectId = new ObjectId(subQuestionId);

                await _formService.AddOptionToSubQuestionAsync(formObjectId, questionObjectId, subQuestionObjectId, option);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }


        [HttpPost("{formId}/questions/{questionId}/options/{optionId}/subQuestion")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SetSubQuestion(string formId, string questionId, string optionId, [FromBody] Question subQuestion)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId) || !ObjectId.TryParse(questionId, out ObjectId questionObjectId) || !ObjectId.TryParse(optionId, out ObjectId optionObjectId))
            {
                return BadRequest("Invalid ID.");
            }

            try
            {
                var savedSubquestion = await _formService.SetSubQuestionAsync(formObjectId, questionObjectId, optionObjectId, subQuestion);
                return Ok(savedSubquestion);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{formId}/autoSetNextQuestions")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AutoSetNextQuestions(string formId)
        {
            if (!ObjectId.TryParse(formId, out ObjectId formObjectId))
            {
                return BadRequest("Invalid ID.");
            }
            try
            {
                await _formService.AutoSetNextQuestions(formObjectId);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }


    }

}
