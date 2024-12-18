﻿using MediatR;
using AudioProcessing.Aplication.MediatR.Chats.CreateChatWithChatResponse;

namespace AudioProcessing.Aplication.MediatR.Chats.CreateStreamingChatWithChatResponse
{
    public class CreateChatWithStreamingChatResponseCommand : IStreamRequest<ChatWithChatResponseDto>
    {
        public string Promt { get; set; } = default;
        public Stream? AudioStream { get; set; }
    }
}
