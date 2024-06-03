package com.example.chatapp_server.service;

import com.example.chatapp_server.ChatMessage;
import com.example.chatapp_server.repository.ChatMessageRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.Comparator;
import java.util.List;
import java.util.Optional;

@Service
public class ChatService {

    private final ChatMessageRepository chatMessageRepository;

    @Autowired
    public ChatService(ChatMessageRepository chatMessageRepository) {
        this.chatMessageRepository = chatMessageRepository;
    }

    public List<ChatMessage> getAllMessages() {
        return chatMessageRepository.findAll();
    }

    public ChatMessage saveMessage(ChatMessage chatMessage) {
        return chatMessageRepository.save(chatMessage);
    }

    public Optional<ChatMessage> getMessageById(int id) {
        return chatMessageRepository.findById(id);
    }

    public ChatMessage updateMessage(int id, ChatMessage chatMessage) {
        if (chatMessageRepository.existsById(id)) {
            chatMessage.setId(id);
            return chatMessageRepository.save(chatMessage);
        }
        return null;
    }

    public boolean deleteMessage(int id) {
        if (chatMessageRepository.existsById(id)) {
            chatMessageRepository.deleteById(id);
            return true;
        }
        return false;
    }

    public ChatMessage getLastMessage() {
        List<ChatMessage> messages = chatMessageRepository.findAll();
        if (messages.isEmpty()) {
            return null;
        }
        return messages.stream().max(Comparator.comparingInt(ChatMessage::getId)).orElse(null);
    }
}
