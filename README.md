# File Storage Microservice

### Demo
    Run $> docker-compose -f docker-compose.yml -d --build
    All the unit tests are executed within Dockerfile. microservice image build will fail if unit tests does'nt pass

    The application swagger json is available at endpoint http://localhost:5000/swagger/v1/swagger.json

    For testing the Application using Swagger UI, naviagte to url http://localhost:5000/swagger/index.html

### Integration Tests
    Run $ docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit
    docker-compose.test.yml utilizes Dockerfile.Test where all the integration tests are executed.

### Deployment