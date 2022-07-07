--
-- PostgreSQL database dump
--

-- Dumped from database version 13.4
-- Dumped by pg_dump version 13.3

-- Started on 2022-02-25 14:21:14

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE document_service;
--
-- TOC entry 3628 (class 1262 OID 24903)
-- Name: document_service; Type: DATABASE; Schema: -; Owner: adnan
--

CREATE DATABASE document_service WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'en_US.utf8';


ALTER DATABASE document_service OWNER TO adnan;

\connect document_service

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: azure_pg_admin
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO azure_pg_admin;

--
-- TOC entry 3629 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: azure_pg_admin
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 203 (class 1259 OID 24953)
-- Name: checked_out_documents; Type: TABLE; Schema: public; Owner: adnan
--

CREATE TABLE public.checked_out_documents (
    _id uuid DEFAULT public.uuid_generate_v1() NOT NULL,
    checked_out_by character varying(32) NOT NULL,
    checked_out_date timestamp with time zone NOT NULL
);


ALTER TABLE public.checked_out_documents OWNER TO adnan;

--
-- TOC entry 201 (class 1259 OID 24935)
-- Name: document; Type: TABLE; Schema: public; Owner: adnan
--

CREATE TABLE public.document (
    _id uuid DEFAULT public.uuid_generate_v1() NOT NULL,
    document_name character varying(32) NOT NULL,
    created_by character varying(128) NOT NULL,
    content character varying(1024) NOT NULL,
    folder_id uuid NOT NULL,
    period_from timestamp with time zone NOT NULL,
    period_to timestamp with time zone NOT NULL,
    version integer NOT NULL
);


ALTER TABLE public.document OWNER TO adnan;

--
-- TOC entry 202 (class 1259 OID 24944)
-- Name: document_history; Type: TABLE; Schema: public; Owner: adnan
--

CREATE TABLE public.document_history (
    _id uuid DEFAULT public.uuid_generate_v1() NOT NULL,
    created_by character varying(128) NOT NULL,
    content bytea NOT NULL,
    period_from timestamp with time zone NOT NULL,
    period_to timestamp with time zone NOT NULL,
    version integer NOT NULL
);


ALTER TABLE public.document_history OWNER TO adnan;

--
-- TOC entry 204 (class 1259 OID 24959)
-- Name: folder; Type: TABLE; Schema: public; Owner: adnan
--

CREATE TABLE public.folder (
    _id uuid DEFAULT public.uuid_generate_v1() NOT NULL,
    folder_name character varying(32) NOT NULL,
    parent_folder_id uuid NOT NULL,
    created_by character varying(32) NOT NULL,
    created_date timestamp with time zone NOT NULL
);


ALTER TABLE public.folder OWNER TO adnan;

--
-- TOC entry 3621 (class 0 OID 24953)
-- Dependencies: 203
-- Data for Name: checked_out_documents; Type: TABLE DATA; Schema: public; Owner: adnan
--

INSERT INTO public.checked_out_documents VALUES ('43c34a30-2819-48bf-b139-14ce84d27331', 'someone@example.com', '2022-02-24 11:30:00+00');


--
-- TOC entry 3619 (class 0 OID 24935)
-- Dependencies: 201
-- Data for Name: document; Type: TABLE DATA; Schema: public; Owner: adnan
--

INSERT INTO public.document VALUES ('81a130d2-502f-4cf1-a376-63edeb000e9f', 'log.txt', 'someone@example.com', '\x5570646174657320746f206c6f672066696c65', '8fceb39d-43a9-4ea5-a44d-e8f225b0bdaa', '2022-02-24 12:00:00+00', '2022-02-24 12:00:00+00', 2);


--
-- TOC entry 3620 (class 0 OID 24944)
-- Dependencies: 202
-- Data for Name: document_history; Type: TABLE DATA; Schema: public; Owner: adnan
--

INSERT INTO public.document_history VALUES ('81a130d2-502f-4cf1-a376-63edeb000e9f', 'someone@example.com', '\x5570646174657320746f206c6f672066696c65', '2022-02-24 11:00:00+00', '2022-02-24 11:00:00+00', 1);


--
-- TOC entry 3622 (class 0 OID 24959)
-- Dependencies: 204
-- Data for Name: folder; Type: TABLE DATA; Schema: public; Owner: adnan
--

INSERT INTO public.folder VALUES ('8fceb39d-43a9-4ae5-a44d-e8f225b0bdaa', 'MyDocuments', '8fceb39d-43a9-4ea5-a44d-e8f225b0bdaa', 'someone@example.com', '2022-02-24 12:00:00+00');


--
-- TOC entry 3486 (class 2606 OID 24958)
-- Name: checked_out_documents checked_out_documents_pkey; Type: CONSTRAINT; Schema: public; Owner: adnan
--

ALTER TABLE ONLY public.checked_out_documents
    ADD CONSTRAINT checked_out_documents_pkey PRIMARY KEY (_id);


--
-- TOC entry 3484 (class 2606 OID 24952)
-- Name: document_history document_history_pkey; Type: CONSTRAINT; Schema: public; Owner: adnan
--

ALTER TABLE ONLY public.document_history
    ADD CONSTRAINT document_history_pkey PRIMARY KEY (_id);


--
-- TOC entry 3482 (class 2606 OID 24943)
-- Name: document document_pkey; Type: CONSTRAINT; Schema: public; Owner: adnan
--

ALTER TABLE ONLY public.document
    ADD CONSTRAINT document_pkey PRIMARY KEY (_id);


--
-- TOC entry 3488 (class 2606 OID 24964)
-- Name: folder folder_pkey; Type: CONSTRAINT; Schema: public; Owner: adnan
--

ALTER TABLE ONLY public.folder
    ADD CONSTRAINT folder_pkey PRIMARY KEY (_id);


--
-- TOC entry 3630 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: azure_pg_admin
--

REVOKE ALL ON SCHEMA public FROM azuresu;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO azure_pg_admin;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2022-02-25 14:21:25

--
-- PostgreSQL database dump complete
--

