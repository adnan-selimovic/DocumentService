export type TFolder = {
  _id: string;
  parent_folder_id?: string;
  folder_id?: string;
  folder_name?: string;
  document_name?: string;
  content_type?: string;
  document_size?: number;
  created_date: string;
  created_by: string;
  path_url: string;
  version?: number;
  checked_out_by?: string;
  checked_out_date?: string;
};
