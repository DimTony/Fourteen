# Build stage
FROM maven:3.9-eclipse-temurin-21 as builder

WORKDIR /app

# Copy pom.xml and source
COPY pom.xml .
COPY src ./src

# Build the application
RUN mvn clean package -DskipTests

# Runtime stage
FROM eclipse-temurin:21-jre-alpine

WORKDIR /app

# Copy the built JAR from builder
COPY --from=builder /app/target/worker-1.0-SNAPSHOT.jar app.jar

# Set environment variables
ENV REDIS_HOST=localhost
ENV REDIS_PORT=6379

# Run the application
ENTRYPOINT ["java", "-jar", "app.jar"]
