# SQS Queue Sample
Before Run:
- Bring up the mocked aws environment for sqs
  - make sure docker installed and run
  - go to project directory
  - run below command in cli
```sh
    docker-compose -f docker-compose-infra.yml up -d
```
- Run multiple startup project
  - right click on solution
  - properties
  - under start up project: run both project

There are 2 application here:
- MyQueue.Publisher : for publish message to SQS
- MyQueue.Consumer : for consume message from SQS
