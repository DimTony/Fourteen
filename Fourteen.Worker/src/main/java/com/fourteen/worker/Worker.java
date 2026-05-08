package com.fourteen.worker;

import redis.clients.jedis.DefaultJedisClientConfig;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisClientConfig;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import javax.net.ssl.SSLSocketFactory;

import com.fourteen.consumer.ScanJobConsumer;

import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

public class Worker {
    private static final Logger LOGGER = Logger.getLogger(Worker.class.getName());

    public static void main(String[] args) {
        String host = System.getenv().getOrDefault("REDIS_HOST", "localhost");
        int port = Integer.parseInt(System.getenv().getOrDefault("REDIS_PORT", "6379"));
        String password = System.getenv().get("REDIS_PASSWORD");

        LOGGER.info("REDIS_HOST=" + host + " REDIS_PORT=" + port);

        // Connectivity pre-check - fail fast with a clear message
        try {
            javax.net.ssl.SSLSocketFactory sslFactory = 
                (javax.net.ssl.SSLSocketFactory) javax.net.ssl.SSLSocketFactory.getDefault();
            try (javax.net.ssl.SSLSocket socket = 
                    (javax.net.ssl.SSLSocket) sslFactory.createSocket(host, port)) {
                socket.setSoTimeout(5000);
                LOGGER.info("TLS connectivity to Redis confirmed");
            }
        } catch (Exception e) {
            LOGGER.severe("Cannot establish TLS connection to Redis at " + host + ":" + port);
            LOGGER.severe("Cause: " + e.getMessage());
            System.exit(1);
        }

        LOGGER.info("Worker starting...");
        LOGGER.info("Connecting to Redis " + host + ":" + port);

        

        JedisPoolConfig poolConfig = new JedisPoolConfig();
        poolConfig.setMaxTotal(10);
        poolConfig.setMaxIdle(5);
        poolConfig.setMinIdle(1);
        poolConfig.setTestOnBorrow(true);
        poolConfig.setTestOnReturn(true);
        poolConfig.setTestWhileIdle(true);

        JedisClientConfig clientConfig = DefaultJedisClientConfig.builder()
            .connectionTimeoutMillis(10_000)
            .socketTimeoutMillis(0)          // must be 0 for brpop blocking
            .password(password)
            .ssl(true)                       // ← this is what was missing
            .build();

        JedisPool jedisPool = new JedisPool(
            poolConfig,
            new HostAndPort(host, port),
            clientConfig
        );

        try {
            new ScanJobConsumer().start(jedisPool);
        } finally {
            jedisPool.destroy();
            LOGGER.info("Worker shutdown complete");
        }
    }
}
