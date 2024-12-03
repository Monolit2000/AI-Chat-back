﻿using AudioProcessing.Aplication.Common.Contract;
using AudioProcessing.Domain.Chats;
using FluentResults;
using MediatR;

namespace AudioProcessing.Aplication.MediatR.Chats.GeneareteChatTitel
{
    public class GeneareteChatTitelCommandHandler(
        IChatRepository chatRepository,
        IOllamaService ollamaService) : IRequestHandler<GeneareteChatTitelCommand, Result<ChatTitelDto>>
    {
        public async Task<Result<ChatTitelDto>> Handle(GeneareteChatTitelCommand request, CancellationToken cancellationToken)
        {

            //var newChatTitel = await ollamaService.GenerateResponce(
            //    new OllamaRequest($"Come up with one name for this chat in accordance with this prompt: {request.Prompt} You must give only one name, that is, your answer must consist of only one word!"));

            var newChatTitel = "testname";

            var chat = await chatRepository.GetByIdAsync(new ChatId(request.ChatId));

            var result = chat.ChengeChatTitel(newChatTitel);

            await chatRepository.SaveChangesAsync(cancellationToken);

            return new ChatTitelDto(chat.Id.Value, newChatTitel);

        }
    }
}
