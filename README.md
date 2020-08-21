# File Storage Microservice

### Demo
    Run $> docker-compose -f docker-compose.yml up -d --build
    All the unit tests are executed within Dockerfile. microservice image build will fail if unit tests does'nt pass

    The application swagger json is available at endpoint http://localhost:5000/swagger/v1/swagger.json

    For testing the Application using Swagger UI, naviagte to url http://localhost:5000/swagger/index.html

### Integration Tests
    Run $ docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit
    docker-compose.test.yml utilizes Dockerfile.Test where all the integration tests are executed.

### Deployment
    To deploy mongo
        step 1: create deployment in local k8s cluster $ kubectl apply -f ./deployments/mongo/deployment.yml
        step 2: create a clusterip service to expose deployment internally $ kubectl apply -f ./deployments/mongo/svc.yml

    To deploy Storage Service
        step 1: build docker image $ docker build -t debapamd/ms_storage:0.0.1 .
        step 2: push image $ docker push debapamd/ms_storage:0.0.1
        step 3: create deployment in local k8s cluster $ kubectl apply -f ./deployments/storage_service/deployment.yml
        step 4: create a external loadbalancer service to expose deployment externally $ kubectl apply -f ./deployments/storage_service/svc.yml