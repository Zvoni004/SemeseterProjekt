package com.example.chatapp_server;

import com.example.chatapp_server.repository.AudioDataRepository;
import com.example.chatapp_server.repository.ChatMessageRepository;
import com.example.chatapp_server.repository.ImageDataRepository;
import com.example.chatapp_server.repository.UserRepository;
import com.example.chatapp_server.service.ChatService;
import com.example.chatapp_server.service.UserService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.web.bind.annotation.*;

import java.util.Base64;
import java.util.Date;
import java.util.List;
import java.util.Map;

@RestController
public class MessageServer {

    @Autowired
    private ChatMessageRepository chatMessageRepository;

    @Autowired
    private AudioDataRepository audioDataRepository;

    @Autowired
    private ImageDataRepository imageDataRepository;

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private UserService userService;

    @Autowired
    private ChatService chatService;
    @Autowired
    private SimpMessagingTemplate messagingTemplate;

    @PostMapping("/sendMessage")
    @SendTo("/topic/messages")
    public ResponseEntity<String> receiveMessage(@RequestBody ChatMessage message) {
        try {
            // Debugging logs
            System.out.println("Received message content: " + message.getMessage());
            System.out.println("Sender: " + message.getSender());

            ChatMessage lastMessage = chatService.getLastMessage();
            if (lastMessage != null) {
                message.setId(lastMessage.getId() + 1);
            } else {
                message.setId(1);
            }

            message.setTimestamp(new Date());


            chatService.saveMessage(message);
            messagingTemplate.convertAndSend("/topic/messages", message);
            return ResponseEntity.ok("Message received successfully!");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Failed to send message: " + e.getMessage());
        }
    }

    @GetMapping("/topic/messages")
    public ResponseEntity<ChatMessage> getMessages() {
        try {
            ChatMessage message = chatService.getLastMessage();
            return ResponseEntity.ok(message);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(null);
        }
    }


    @PostMapping("/sendAudio")
    @SendTo("/topic/audios")
    public ResponseEntity<String> receiveAudio(@RequestBody AudioDataRequest audioDataRequest) {
        try {
            byte[] decodedAudioData = Base64.getDecoder().decode(audioDataRequest.getAudioBase64());
            AudioData audioData = new AudioData();
            audioData.setData(decodedAudioData);
            audioData.setSender(audioDataRequest.getSender());
            audioData.setTimestamp(new Date());
            audioDataRepository.save(audioData);
            messagingTemplate.convertAndSend("/topic/audio", audioData);
            return ResponseEntity.ok("Audio received successfully!");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Failed to receive audio: " + e.getMessage());
        }
    }

    //Funktioniert noch nicht richtig, deshalb auskommentiert, dass selbe gilt für andere Funktionen (nicht gesschafft)
   /* @GetMapping("/topic/audios")
    public ResponseEntity<List<AudioData>> getAudios() {
        try {
            List<AudioData> audios = audioDataRepository.findAll();
            return ResponseEntity.ok(audios);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(null);
        }
    }
*/
    @PostMapping("/sendImage")
    @SendTo("/topic/images")
    public ResponseEntity<String> receiveImage(@RequestBody ImageDataRequest imageDataRequest) {
        try {
            byte[] decodedImageData = Base64.getDecoder().decode(imageDataRequest.getImageBase64());
            ImageData imageData = new ImageData();
            imageData.setData(decodedImageData);
            imageData.setSender(imageDataRequest.getSender());
            imageData.setTimestamp(new Date());
            imageDataRepository.save(imageData);
            messagingTemplate.convertAndSend("/topic/images", imageData);
            return ResponseEntity.ok("Image received successfully!");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Failed to receive image: " + e.getMessage());
        }
    }

  /*  @GetMapping("/topic/images")
    public ResponseEntity<List<ImageData>> getImages() {
        try {
            List<ImageData> images = imageDataRepository.findAll();
            return ResponseEntity.ok(images);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(null);
        }
    }
*/
    @PostMapping("/register")
    public ResponseEntity<String> registerUser(@RequestBody User user) {
        try {
            User existingUser = userRepository.findByUsername(user.getUsername());
            if (existingUser != null) {
                return ResponseEntity.status(HttpStatus.CONFLICT).body("Username already exists");
            }

            userService.saveUser(user);
            return ResponseEntity.status(HttpStatus.CREATED).body("User registered successfully");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Registration failed: " + e.getMessage());
        }
    }

    @PostMapping("/login")
    public ResponseEntity<String> loginUser(@RequestBody Map<String, String> user) {
        String username = user.get("Username");
        String password = user.get("Password");

        if (userService.validateUser(username, password)) {
            return ResponseEntity.ok("Login successful");
        } else {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).body("Invalid username or password");
        }
    }


    static class AudioDataRequest {
        private String audioBase64;
        private String sender;

        public String getAudioBase64() {
            return audioBase64;
        }

        public void setAudioBase64(String audioBase64) {
            this.audioBase64 = audioBase64;
        }

        public String getSender() {
            return sender;
        }

        public void setSender(String sender) {
            this.sender = sender;
        }
    }

    static class ImageDataRequest {
        private String imageBase64;
        private String sender;

        public String getImageBase64() {
            return imageBase64;
        }

        public void setImageBase64(String imageBase64) {
            this.imageBase64 = imageBase64;
        }

        public String getSender() {
            return sender;
        }

        public void setSender(String sender) {
            this.sender = sender;
        }
    }
}
