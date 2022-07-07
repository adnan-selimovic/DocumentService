import { UserPermissionsEnum } from '../enums/user-permissions.enum';

export class UserPermissionsClass {
  userPermissions!: string[] | undefined;

  constructor(permissions: string[] | undefined) {
    this.userPermissions = permissions;
  }

  get allPermissions(): string[] | undefined {
    return this.userPermissions;
  }

  get hasPermission(): boolean {
    const permissions =
      this.userPermissions?.filter((el) => el !== UserPermissionsEnum.READ) ||
      [];
    return permissions.length > 0 || false;
  }

  get isOwner(): boolean {
    return (
      this.userPermissions?.some((el) => el === UserPermissionsEnum.OWNER) ||
      false
    );
  }

  get hasAll(): boolean {
    return (
      this.userPermissions?.some((el) => el === UserPermissionsEnum.ALL) ||
      false
    );
  }

  get canRead(): boolean {
    return (
      this.isOwner ||
      this.hasAll ||
      this.userPermissions?.some((el) => el === UserPermissionsEnum.READ) ||
      false
    );
  }

  get canWrite(): boolean {
    return (
      this.isOwner ||
      this.hasAll ||
      this.userPermissions?.some((el) => el === UserPermissionsEnum.WRITE) ||
      false
    );
  }

  get canDelete(): boolean {
    return (
      this.isOwner ||
      this.hasAll ||
      this.userPermissions?.some((el) => el === UserPermissionsEnum.DELETE) ||
      false
    );
  }
}
