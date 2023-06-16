# user-service
Простейший RESTful CRUD с деплоем в кубер с использование helm

Перед установкой необходимо установить Postgresql

# Установка Postgresql:
helm repo add bitnami https://charts.bitnami.com/bitnami && helm repo update && helm install postgresql bitnami/postgresql --set auth.username=userservice,auth.password=password,auth.database=UserServiceDb


# Установка user-service:
helm repo add user-service-repo https://NikolayBezdenezhnykh.github.io/user-service/charts && helm install us-app user-service-repo/user-service