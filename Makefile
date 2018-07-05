default:
	@echo No default target

build:
	docker build -t demo:1 .

run:
	docker run -it -p 5000:5000 demo:latest

up:
	docker-compose up --build --force-recreate --scale worker=3

stop:
	docker-compose stop

deploy:
	docker deploy -c docker-compose.yml demo

logs:
	docker service logs -f demo_worker