using MediatR;
using Microsoft.AspNetCore.Mvc;
using AudioProcessing.Aplication.MediatR.Chats.CreateChat;
using AudioProcessing.Aplication.MediatR.Chats.DeleteChat;
using AudioProcessing.Aplication.MediatR.Chats.GetAllChats;
using AudioProcessing.Aplication.MediatR.Chats.CreateTrancription;
using AudioProcessing.Aplication.MediatR.Chats.GetAllChatsByUserId;
using AudioProcessing.Aplication.MediatR.Chats.GetAllChatResponses;
using AudioProcessing.Aplication.MediatR.Chats.GetAllChatResponsesByChatId;
using AudioProcessing.Aplication.MediatR.Chats.CreateChatWithChatResponse;
using AudioProcessing.Aplication.MediatR.Chats.CreateChatResponseOnText;
using AudioProcessing.Aplication.MediatR.Chats.ChegeChatTitel;
using AudioProcessing.Aplication.Services.Ollama;
using AudioProcessing.Aplication.Common.Contract;
//using Newtonsoft.Json;
using System.Net.Http;
using AudioProcessing.Aplication.MediatR.Chats.GeneareteChatTitel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using AudioProcessing.Aplication.Services.Chats;
using System.Text.Json;

namespace AudioProcessing.API.Controllers
{

    public class ModelResponse
    {
        public string[] Models { get; set; }
    }



    [ApiController]
    [Route("[controller]")]
    public class AudioProcessing : ControllerBase
    {
        private IMediator _mediator;
        private IOllamaService _ollamaService;
        private HttpClient _httpClient;
        private IChatStreamingService _chatStreamingService;

        public AudioProcessing(
            IMediator mediator, 
            IOllamaService ollamaService, 
            HttpClient httpClient,
            IChatStreamingService chatStreamingService)
        {
            _mediator = mediator;   
            _ollamaService = ollamaService;
            _httpClient = httpClient;
            _chatStreamingService = chatStreamingService;
        }


        [HttpGet("stream-sse")]
        public async Task StreamAsync(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";

            await foreach (var response in _ollamaService.GenerateStreameTextContentResponce(new OllamaRequest("send big leter on 200 words")))
            {
                await Response.WriteAsync($"data: {response?.Response}\n\n");
                //await Task.Delay(200);
                Console.WriteLine($"Sending: {response?.Response}");
                await Response.Body.FlushAsync(); 
            }
        }



        [HttpPost("createStreamingChatResponseOnText")]
        public async Task CreateStreamingChatResponseOnText([FromBody] CreateChatResponseOnTextCommand createChatResponseOnTextCommand/*, Guid userId*/)
        {
            Response.ContentType = "text/event-stream";

            await foreach (var response in _chatStreamingService.CreateStreamingChatResponseOnText(createChatResponseOnTextCommand))
            {
                var json = JsonSerializer.Serialize(response);

                await Response.WriteAsync($"data: {json}\n\n");
                //await Task.Delay(200);
                Console.WriteLine($"Sending: {response?.Conetent}");
                await Response.Body.FlushAsync();
            }
        }






        [HttpGet("streamChat-sse")]
        public async Task StreamChatAsync(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";

            await foreach (var response in _ollamaService.GenerateStreamingChatResponse(new OllamaRequest("send big leter on 200 words"), ["string"]))
            {
                await Response.WriteAsync($"data: {response.Content}\n\n");
                //await Task.Delay(200);
                Console.WriteLine($"Sending: {response.Content}");
                await Response.Body.FlushAsync();
            }
        }



      


        [HttpPost("generate-response")]
        public async Task<IActionResult> GenerateResponse([FromBody] OllamaRequest request)
        {
            var responseText = await _ollamaService.GenerateTextContentResponce(request);
            return Ok(new { Response = responseText });
        }


        [HttpPost("geneareteChatTitel")]
        public async Task<IActionResult> GeneareteChatTitelCommand([FromBody] GeneareteChatTitelCommand geneareteChatTitelCommand)
        {
            var result = await _mediator.Send(geneareteChatTitelCommand);

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);
        }




        [HttpPost("createChatResponseOnText")]
        public async Task<IActionResult> CreateChatResponseOnText([FromBody] CreateChatResponseOnTextCommand createChatResponseOnTextCommand/*, Guid userId*/)
        {
            var result = await _mediator.Send(createChatResponseOnTextCommand);

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);
        }





        [HttpPost("createTrancription")]
        [RequestSizeLimit(100000000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTrancription([FromForm] ChatRequest chatRequest/*, Guid userId*/)
        {

            var command = new CreateTrancriptionCommmand
            {
                ChatId =  Guid.Parse(chatRequest.ChatId),
                AudioStream = chatRequest.AudioFile.OpenReadStream(),
                Promt = chatRequest.Promt
            };

            var result = await _mediator.Send(command);

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);   
        } 

        
        [HttpPost("createChatWithChatResponse")]
        [RequestSizeLimit(100000000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateChatWithChatResponseCommand([FromForm] CreateChatWithChatResponsReques createChatWithChatResponsRequest/*, Guid userId*/)
        {

            var command = new CreateChatWithChatResponseCommand
            {
                AudioStream = createChatWithChatResponsRequest.AudioFile?.OpenReadStream(),
                Promt = createChatWithChatResponsRequest.Promt
            };

            var result = await _mediator.Send(command);

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);   
        } 






        
        //[HttpPost("createChatWithTranscription")]
        //[RequestSizeLimit(100000000)]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> CreateChatWithTranscription(IFormFile audioFile)
        //{
        //    var command = new CreateChatWithChatResponseCommand 
        //    { 
        //        AudioStream = audioFile.OpenReadStream()
        //    };

        //    var rsult = await _mediator.Send(command);
        //    return Ok(rsult.Value);   
        //}



        [HttpPost("renameChat")]
        public async Task<IActionResult> RenameChat([FromBody] ChegeChatTitelCommand chegeChatTitelCommand/*, Guid userId*/)
        {
            var result = await _mediator.Send(chegeChatTitelCommand);

            if (result.IsFailed)
                return BadRequest();

            return Ok();
        }


        [HttpPost("createNewChat")]
        public async Task<IActionResult> CreateChat()
        {
            var result = await _mediator.Send(new CreateChatCommand());

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);
        }

        [HttpPost("deleteChat")]
        public async Task<IActionResult> DeleteChat([FromBody] DeleteChatRequest deleteChatRequest)
        {
            var result = await _mediator.Send(new DeleteChatCommand { ChatId = deleteChatRequest.ChatId/*Guid.Parse(chatId)*/ });

            if (result.IsFailed)
                return BadRequest();

            return Ok(result);
        }



        [HttpGet("GetAllChatsByUserId")]
        public async Task<IActionResult> GetAllChatsByUserId([FromQuery] GetAllChatsByUserIdQuery getAllChatsByUserIdQuery)
        {
            var rsult = await _mediator.Send(getAllChatsByUserIdQuery);

            return Ok(rsult);   
        }


        [HttpGet("getAllChatResponsesByChatId/{chatId}")]
        public async Task<IActionResult> GetAllChatsByUserId(string chatId)
        {
            var result = await _mediator.Send(new GetAllChatResponsesByChatIdQuery() { ChatId = Guid.Parse(chatId)});

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);   
        }

        [HttpGet("getAllChatResponses")]
        public async Task<IActionResult> GetAllChatsByUserId()
        {
            var result = await _mediator.Send(new GetAllChatResponsesQuery());

            if (result.IsFailed)
                return BadRequest();

            return Ok(result.Value);   
        }




        [HttpGet("GetAllChats")]
        public async Task<IActionResult> GetAllChats()
        {
            var rsult = await _mediator.Send(new GetAllChatsQuery());

            return Ok(rsult);   
        } 
    }
}
