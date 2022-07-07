import { AuthResponseModel } from '../../../app-core/models';

export class AuthStateModel {
  loading!: boolean;
  error: any;
  auth!: AuthResponseModel | null;
}

export const defaultState: AuthStateModel = {
  loading: false,
  error: null,
  auth: null,
};
