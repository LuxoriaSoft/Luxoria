-- Create a new database user
CREATE USER dbowner WITH PASSWORD 'strongpassword';

-- Grant ownership of the database to the user
ALTER DATABASE luxoria OWNER TO dbowner;

-- Grant all privileges on all tables in luxoria to the user
\c luxoria;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO dbowner;

-- Allow the user to manage sequences
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO dbowner;

-- Allow the user to manage schemas
GRANT USAGE, CREATE ON SCHEMA public TO dbowner;
