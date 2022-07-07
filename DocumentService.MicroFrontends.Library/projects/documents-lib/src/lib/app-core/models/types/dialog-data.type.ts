import { ErrorResponse } from '../classes/error-response.class';

export class DialogData {
  title!: string;
  description?: string;
  onClose?: () => void;
  onConfirm?: () => void;
  showConfirm? = false;
  showClose? = true;

  data?: ErrorResponse[];
}
