import { Component, OnInit } from '@angular/core';
import { Select, Store } from '@ngxs/store';
import {
  DialogData,
  FolderPath,
  UserPermissionType,
  UserPermissionTypeEnum,
} from '../../../../app-core/models';
import { Observable } from 'rxjs';
import { DocumentsSelectors } from '../../store/documents.selectors';
import { FormBuilder, Validators } from '@angular/forms';
import { ConfirmActionMessageComponent } from '../../../../shared/global-components/messages/confirm-action-message/confirm-action-message.component';
import { MatDialog } from '@angular/material/dialog';
import {
  AddFolderPathPermissions,
  DeleteFolderPathPermissions,
  UpdateUserPermissionOnFolderPath,
} from '../../store/documents.actions';
import { UserPermissionsEnum } from '../../../../app-core/models/enums/user-permissions.enum';

@Component({
  selector: 'lib-user-permissions-layout',
  templateUrl: './user-permissions-layout.component.html',
  styleUrls: ['./user-permissions-layout.component.css'],
})
export class UserPermissionsLayoutComponent implements OnInit {
  @Select(DocumentsSelectors.getFolderPath)
  getFolderPath$!: Observable<FolderPath>;
  @Select(DocumentsSelectors.getFolderPathPermissions)
  getFolderPathPermissions$!: Observable<UserPermissionType[]>;

  getFolderPath!: FolderPath;

  form = this.fb.group({
    userId: ['', Validators.required],
  });
  get userId() {
    return this.form.get('userId');
  }

  // owner
  PERMISSIONS = ['all', 'read', 'write', 'delete'];
  USERS_PERMISSIONS: UserPermissionType[] = [];
  UserPermissionTypeEnum = UserPermissionTypeEnum;
  userType = UserPermissionTypeEnum.USER;

  constructor(
    private fb: FormBuilder,
    private dialog: MatDialog,
    private store: Store
  ) {}

  ngOnInit(): void {
    this.getFolderPath$.subscribe((values) => {
      this.getFolderPath = values;
    });
    this.getFolderPathPermissions$.subscribe((values) => {
      this.USERS_PERMISSIONS = values;
    });
  }

  setUserType(userType: UserPermissionTypeEnum) {
    this.userType = userType;
  }

  addNew(): void {
    const userId = this.userId?.value;

    const checkIfExists = this.USERS_PERMISSIONS.find(
      (u) => u.userId === userId
    );
    if (checkIfExists) {
      this.showMsg(
        'Check Your Input!',
        `${
          this.userType === UserPermissionTypeEnum.USER ? 'User' : 'Group'
        } "${userId}" already exists. Please try again.`
      );
      return;
    }

    this.form.reset();
    this.userId?.reset();
    this.submitPermissions({
      userId: userId,
      permissions: ['read'],
      isGroup: this.userType === UserPermissionTypeEnum.GROUP,
    }).subscribe({
      next: () => {
        this.USERS_PERMISSIONS = [
          {
            userId: userId,
            permissions: ['read'],
            isGroup: this.userType === UserPermissionTypeEnum.GROUP,
          },
          ...this.USERS_PERMISSIONS,
        ];
      },
      error: () => null,
    });
  }
  updatePermissions(permission: string, index: number): void {
    if (permission === UserPermissionsEnum.READ) {
      return;
    }

    let currentUser = this.USERS_PERMISSIONS[index];
    const find = currentUser.permissions.find((p) => p === permission);
    if (find) {
      const permissions = currentUser.permissions.filter((p) => p !== find);
      currentUser = { ...currentUser, permissions };
    } else {
      currentUser = {
        ...currentUser,
        permissions: [...currentUser.permissions, permission],
      };
    }

    let userPermissions = [...this.USERS_PERMISSIONS];
    userPermissions[index] = currentUser;
    this.updateUserPermission(currentUser).subscribe({
      next: () => {
        this.USERS_PERMISSIONS = userPermissions;
      },
      error: () => null,
    });
  }

  deleteUserPermissions(index: number): void {
    const currentUser = this.USERS_PERMISSIONS[index];
    let dialogData: DialogData;
    dialogData = {
      title: 'Confirm Your Action!',
      description: `Are you sure you want to delete "${currentUser.userId}" permissions?`,
      showConfirm: true,
      onConfirm: () => {
        let userPermissions = [...this.USERS_PERMISSIONS];
        userPermissions.splice(index, 1);
        this.USERS_PERMISSIONS = userPermissions;
        this.store
          .dispatch(
            new DeleteFolderPathPermissions(
              this.getFolderPath.folderPath,
              currentUser.userId
            )
          )
          .subscribe(() => {
            this.dialog.closeAll();
          });
      },
    };
    this.dialog.open(ConfirmActionMessageComponent, {
      width: '50%',
      data: dialogData,
    });
  }

  submitPermissions(permissionObj: UserPermissionType): Observable<any> {
    return this.store.dispatch(
      new AddFolderPathPermissions(this.getFolderPath.folderPath, permissionObj)
    );
  }
  updateUserPermission(permissionObj: UserPermissionType): Observable<any> {
    return this.store.dispatch(
      new UpdateUserPermissionOnFolderPath(
        this.getFolderPath.folderPath,
        permissionObj
      )
    );
  }

  showMsg(title: string, description: string) {
    let dialogData: DialogData;
    dialogData = {
      title,
      description,
      showConfirm: false,
    };
    this.dialog.open(ConfirmActionMessageComponent, {
      width: '50%',
      data: dialogData,
    });
  }

  checkPermission(userPermissions: string[], permission: string): boolean {
    return userPermissions.find((p) => p === permission) ? true : false;
  }
}
