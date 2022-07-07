export type TDocument = {
  _id: string;
  document_name: string;
  content_type: string;
  document_size: number;
  path_url: string;
  created_by: string;
  created_date: string;
  folder_id: string;
  version: number;
  checked_out_by: string;
  checked_out_date: string;

  text_content?: string;
};
