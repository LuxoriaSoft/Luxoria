-- Already created by POSTGRES_DB env var, no need to recreate
-- CREATE DATABASE luxoria;

\c luxoria;

CREATE USER dbowner WITH PASSWORD 'strongpassword';
ALTER DATABASE luxoria OWNER TO dbowner;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO dbowner;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO dbowner;
GRANT USAGE, CREATE ON SCHEMA public TO dbowner;
