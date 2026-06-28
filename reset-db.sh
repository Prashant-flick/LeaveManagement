#!/bin/bash

# Set database variables
DB_CONTAINER_NAME="postgres-db"
DB_PORT="5432"
DB_USER="postgres"
DB_PASSWORD="Prashant1251@"
DB_NAME="employeeleave"

echo "Checking if postgres container is running..."
if [ ! "$(docker ps -q -f name=$DB_CONTAINER_NAME)" ]; then
    if [ "$(docker ps -aq -f status=exited -f name=$DB_CONTAINER_NAME)" ]; then
        echo "Starting existing postgres container ($DB_CONTAINER_NAME)..."
        docker start $DB_CONTAINER_NAME
    else
        echo "Creating and starting new postgres container ($DB_CONTAINER_NAME)..."
        docker run --name $DB_CONTAINER_NAME -e POSTGRES_PASSWORD=$DB_PASSWORD -p $DB_PORT:5432 -d postgres:latest
        echo "Waiting for postgres database to start up..."
        sleep 5
    fi
fi

echo "Dropping and recreating database: $DB_NAME..."
# Force terminate other active connections to prevent lock errors
docker exec -e PGPASSWORD=$DB_PASSWORD $DB_CONTAINER_NAME psql -U $DB_USER -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();"
docker exec -e PGPASSWORD=$DB_PASSWORD $DB_CONTAINER_NAME psql -U $DB_USER -c "DROP DATABASE IF EXISTS $DB_NAME;"
docker exec -e PGPASSWORD=$DB_PASSWORD $DB_CONTAINER_NAME psql -U $DB_USER -c "CREATE DATABASE $DB_NAME;"

echo "=========================================================="
echo "Database reset completed successfully."
echo "=========================================================="
echo "To migrate and seed the database on service startup:"
echo "Configure \"InitStrategy\": \"Recreate\" in your appsettings.json or appsettings.local.json,"
echo "and then start the backend services."
