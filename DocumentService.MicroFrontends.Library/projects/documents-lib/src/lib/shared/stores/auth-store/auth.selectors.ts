import { Selector } from '@ngxs/store';
import { AuthResponseModel } from '../../../app-core/models';
import { AuthStateModel } from './auth-state.model';
import { AuthState } from './auth.state';

export class AuthSelectors {
  @Selector([AuthState])
  public static getAuth(state: AuthStateModel): AuthResponseModel | null {
    return state.auth;
  }
}
