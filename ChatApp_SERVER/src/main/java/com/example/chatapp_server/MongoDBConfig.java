package com.example.chatapp_server;

import com.mongodb.client.MongoClient;
import com.mongodb.client.MongoClients;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.mongodb.config.AbstractMongoClientConfiguration;
import org.springframework.data.mongodb.repository.config.EnableMongoRepositories;

@Configuration
@EnableMongoRepositories(basePackages = "com.example.chatapp_server.repository")
public class MongoDBConfig extends AbstractMongoClientConfiguration {

    @Override
    protected String getDatabaseName() {
        return "ChatAppDB";
    }

    @Override
    protected String getMappingBasePackage() {
        return "com.example.chatapp_server";
    }

    @Override
    public MongoClient mongoClient() {
        String connectionString = "mongodb+srv://shadow_04er:JebemtiPasswort187!@cluster0.al7nfkc.mongodb.net/ChatAppDB";
        return MongoClients.create(connectionString);
    }
}
