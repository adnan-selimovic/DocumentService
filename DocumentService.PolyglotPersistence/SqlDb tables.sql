create table document (
	_id uuid PRIMARY KEY DEFAULT uuid_generate_v1(),
	document_name varchar(32) not null,
	created_by varchar(128) not null,
	content varbinary(8000) not null,
	folder_id uuid not null,
	period_from timestamp not null,
	period_to timestamp not null,
	version int not null
)

create table document_history (
	_id uuid PRIMARY KEY DEFAULT uuid_generate_v1(),
	created_by varchar(128) not null,
	content varbinary(8000) not null,
	period_from timestamp not null,
	period_to timestamp not null,
	version int not null
)

create table checked_out_documents (
	_id uuid PRIMARY KEY DEFAULT uuid_generate_v1(),
	checked_out_by varchar(32) not null,
	checked_out_date timestamp not null
)

create table folder (
	_id uuid PRIMARY KEY DEFAULT uuid_generate_v1(),
	folder_name varchar(32) not null,
	parent_folder_id uuid not null,
	created_by varchar(32) not null,
	created_date timestamp not null
)


insert into document ("_id", "document_name", "created_by", "content", "folder_id", "period_from", "period_to", "version")
values ('81a130d2-502f-4cf1-a376-63edeb000e9f', 'log.txt', 'someone@example.com', 'Updates to log file', '8fceb39d-43a9-4ea5-a44d-e8f225b0bdaa', '2022-02-24T12:00:00Z', '2022-02-24T12:00:00Z', 2)

insert into document_history ("_id", "created_by", "content", "period_from", "period_to", "version")
values ('81a130d2-502f-4cf1-a376-63edeb000e9f', 'someone@example.com', 'Updates to log file', '2022-02-24T11:00:00Z', '2022-02-24T11:00:00Z', 1)

insert into checked_out_documents ("_id", "checked_out_by", "checked_out_date")
values ('43c34a30-2819-48bf-b139-14ce84d27331', 'someone@example.com', '2022-02-24T11:30:00Z')

insert into folder ("_id", "folder_name", "parent_folder_id", "created_by", "created_date")
values ('8fceb39d-43a9-4ae5-a44d-e8f225b0bdaa', 'MyDocuments', '8fceb39d-43a9-4ea5-a44d-e8f225b0bdaa', 'someone@example.com', '2022-02-24T12:00:00Z')