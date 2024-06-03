package com.example.chatapp_server.repository;

import com.example.chatapp_server.ImageData;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface ImageDataRepository extends MongoRepository<ImageData, String> {

}