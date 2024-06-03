package com.example.chatapp_server;

import com.example.chatapp_server.ChatMessage;
import com.example.chatapp_server.service.ChatService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/messages")
public class ChatMessageController {

    private final ChatService chatService;

    @Autowired
    public ChatMessageController(ChatService chatService) {
        this.chatService = chatService;
    }

    @GetMapping
    public List<ChatMessage> getAllMessages() {
        return chatService.getAllMessages();
    }

    @PostMapping
    public ChatMessage createMessage(@RequestBody ChatMessage chatMessage) {
        return chatService.saveMessage(chatMessage);
    }

    @GetMapping("/{id}")
    public Optional<ChatMessage> getMessageById(@PathVariable String id) {
        return chatService.getMessageById(Integer.parseInt(id));
    }

    @PutMapping("/{id}")
    public ChatMessage updateMessage(@PathVariable String id, @RequestBody ChatMessage chatMessage) {
        return chatService.updateMessage(Integer.parseInt(id), chatMessage);
    }

    @DeleteMapping("/{id}")
    public boolean deleteMessage(@PathVariable String id) {
        return chatService.deleteMessage(Integer.parseInt(id));
    }
}
