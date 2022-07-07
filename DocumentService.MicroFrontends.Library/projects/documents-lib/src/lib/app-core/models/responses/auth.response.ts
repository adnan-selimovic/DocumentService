export class AuthResponseModel {
  access_token!: string;
  expires_in!: number;
  token_type!: string;
  scope!: string;
  expires_on?: Date;
}
