﻿using MediatR;
using FluentResults;
using AudioProcessing.Domain.Chats;
using AudioProcessing.Domain.Users;
using AudioProcessing.Aplication.Common.Contract;
using AudioProcessing.Aplication.Common.Models;
using System.Text;
using Azure.Core;

namespace AudioProcessing.Aplication.MediatR.Chats.CreateChatWithChatResponse
{
    public class CreateChatWithChatResponseCommandHandler(
        IOllamaService ollamaService,
        IChatRepository chatRepository,
        IAudioProcessingService audioProcessingService) : IRequestHandler<CreateChatWithChatResponseCommand, Result<ChatWithChatResponseDto>>
    {
        public async Task<Result<ChatWithChatResponseDto>> Handle(CreateChatWithChatResponseCommand request, CancellationToken cancellationToken)
        {
            //var transcriptionResult = await audioProcessingService.CreateTranscription(request.AudioStream);

            var transcriptionResult = await HendlePromptAsync(request.Promt);

            var prompt = FielterPrompt(request.Promt);

            var chat = Chat.Create(new UserId(Guid.NewGuid()), "New сhat");

            chat.AddChatResponseOnText(transcriptionResult.Text, prompt);

            await chatRepository.AddAsync(chat, cancellationToken);
            await chatRepository.SaveChangesAsync(cancellationToken);

            var chatDto = new ChatDto(
               chat.Id.Value,
               chat.ChatTitel,
               chat.CreatedDate);

            return new ChatWithChatResponseDto(chatDto, transcriptionResult.Text, prompt);

        }


        private async Task<AudioTranscriptionResponce> HendlePromptAsync(string prompt)
        {
            var defaultContent = $"CreateTrancriptionCommmand {Guid.NewGuid().ToString()} +" +
           $" Promt: {(string.IsNullOrWhiteSpace(prompt) ? "None" : prompt)}";

            AudioTranscriptionResponce transcriptionResult;

            if (!string.IsNullOrWhiteSpace(prompt) && prompt.Trim().StartsWith("@"))
            {
                prompt = prompt.Trim().Substring(1).Trim();

                var specialResponse = await ollamaService.GenerateTextContentResponce(new OllamaRequest(prompt));
                transcriptionResult = new AudioTranscriptionResponce(specialResponse);
            }
            else
            {
                transcriptionResult = new AudioTranscriptionResponce(defaultContent);
            }

            return transcriptionResult; 
        }

        private string FielterPrompt(string prompt)
        {
            if (!string.IsNullOrWhiteSpace(prompt) && prompt.Trim().StartsWith("@"))
            {
                prompt = prompt.Trim().Substring(1).Trim();
            }

            return prompt;
        }
    }
}

/* (prompt.Trim().StartsWith("@o", StringComparison.OrdinalIgnoreCase)*/ 