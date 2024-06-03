package com.example.chatapp_server.repository;

import com.example.chatapp_server.AudioData;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface AudioDataRepository extends MongoRepository<AudioData, String> {

}
