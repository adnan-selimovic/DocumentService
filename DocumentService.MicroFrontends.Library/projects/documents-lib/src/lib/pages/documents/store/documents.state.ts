import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Action, NgxsOnInit, State, StateContext } from '@ngxs/store';
import { catchError, finalize, Observable, of, tap, throwError } from 'rxjs';
import {
  checkIfPathExistsInFolderHierarchy,
  checkIfTreeExpanedOnPath,
  findChildrenFolderOnPath,
  updateTreeWithChildren,
} from '../../../app-core/helpers';
import {
  ErrorResponse,
  FolderPath,
  FolderPreviewType,
  StorageEnum,
  RDocuments,
  UploadDocumentResponse,
  FolderOptions,
  UserPermissionType,
  TDocument,
  DocumentHelperClass,
} from '../../../app-core/models';
import { DocumentService } from '../../../app-core/services/document.service';
import { StorageService } from '../../../app-core/utils/storage.service';
import { ErrorMessageComponent } from '../../../shared/global-components/messages/error-message/error-message.component';
import { defaultState, DocumentsStateModel } from './documents-state.model';
import {
  AddFolderPathPermissions,
  CheckInDocument,
  CheckOutDocument,
  CreateFolder,
  DeleteDocument,
  DeleteFolder,
  DeleteFolderPathPermissions,
  DownloadDocument,
  GetDocumentTextContent,
  GetFolderPathPermissions,
  LoadDocuments,
  LoadFolderHierarchy,
  LoadPublicDocuments,
  SearchFoldersHierarchy,
  SearchThroughDocuments,
  SelectDocument,
  SetFolderOptions,
  SetPathAccessType,
  UpdateDocument,
  UpdateUserPermissionOnFolderPath,
  UploadDocument,
} from './documents.actions';
import { FileSaverService } from 'ngx-filesaver';

@State<DocumentsStateModel>({
  name: 'documents',
  defaults: defaultState,
})
@Injectable()
export class DocumentsState implements NgxsOnInit {
  constructor(
    private _documentService: DocumentService,
    private _storageService: StorageService,
    private dialog: MatDialog,
    private router: Router,
    private _fileSaverService: FileSaverService
  ) {}

  ngxsOnInit(ctx?: StateContext<DocumentsStateModel>) {
    const folderOptions = this._storageService.getItem<FolderOptions>(
      StorageEnum.FOLDER_OTPIONS
    );
    ctx?.patchState({
      options: folderOptions || { previewType: FolderPreviewType.FOLDER },
    });
  }

  @Action(LoadDocuments)
  LoadDocuments(
    ctx: StateContext<DocumentsStateModel>,
    action: LoadDocuments
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const foldersTree = ctx.getState().foldersTree;
    const rootFolder = ctx.getState().rootFolder;

    const checkIfHierarchyExists = foldersTree
      ? action.folderPath === '/'
        ? true
        : checkIfPathExistsInFolderHierarchy(foldersTree, action.folderPath)
      : false;

    if (!checkIfHierarchyExists) {
      ctx.dispatch(new LoadFolderHierarchy(action.folderPath));
    }
    const isCurrentlyLoaded = rootFolder?.rootFolderPath === action.folderPath;
    if (isCurrentlyLoaded && !action.forceLoad) {
      return of();
    }

    return this._documentService.getDocuments(action.folderPath).pipe(
      tap((response: RDocuments) => {
        const isEmpty =
          response.documents.length === 0 && response.folders.length === 0;
        ctx.patchState({
          loading: false,
          folderPath: new FolderPath(
            action.folderPath,
            action.folderPath === '/'
              ? [action.folderPath]
              : [...action.folderPath.split('/')]
          ),
          rootFolder: {
            ...response,
            rootFolderPath: action.folderPath,
            isEmpty,
          },
        });
      }),
      catchError((err) => {
        this.router.navigateByUrl('/not-found');
        ctx.patchState({ error: err });
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  @Action(LoadPublicDocuments)
  LoadPublicDocuments(
    ctx: StateContext<DocumentsStateModel>,
    action: LoadPublicDocuments
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });

    return this._documentService.getPublicDocuments(action.folderPath).pipe(
      tap((response: RDocuments) => {
        const isEmpty =
          response.documents.length === 0 && response.folders.length === 0;
        ctx.patchState({
          loading: false,
          folderPath: new FolderPath(
            action.folderPath,
            action.folderPath === '/'
              ? [action.folderPath]
              : [...action.folderPath.split('/')]
          ),
          publicFolder: {
            ...response,
            rootFolderPath: action.folderPath,
            isEmpty,
          },
        });
      }),
      catchError((err) => {
        this.router.navigateByUrl('/not-found');
        ctx.patchState({ error: err });
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  @Action(LoadFolderHierarchy)
  LoadFolderHierarchy(
    ctx: StateContext<DocumentsStateModel>,
    action: LoadFolderHierarchy
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentTree = ctx.getState().foldersTree;
    return this._documentService.getFolderHierarchy(action.folderPath).pipe(
      tap((response: any) => {
        let newTree = response;
        const findNewChildren = findChildrenFolderOnPath(
          newTree,
          action.folderPath
        );
        if (findNewChildren && currentTree) {
          const currentTreeCheck = checkIfTreeExpanedOnPath(
            currentTree,
            action.folderPath
          );
          newTree = updateTreeWithChildren(
            currentTreeCheck,
            findNewChildren.childFolder,
            action.folderPath
          );
        }
        ctx.patchState({
          loading: false,
          foldersTree: newTree,
        });
      }),
      catchError((err) => {
        ctx.patchState({ error: err });
        this.handleError('Something Bad Happened', [
          { status: 0, message: 'Cannot fetch the data.', code: '' },
        ]);
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  @Action(UploadDocument)
  UploadDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: UploadDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const folderpath =
      action.folderPath != null
        ? action.folderPath
        : ctx.getState().folderPath.folderPath;
    return this._documentService.uploadDocument(folderpath, action.files).pipe(
      tap((response: UploadDocumentResponse) => {
        if (response.numberOfFailed > 0) {
          this.handleError(
            'Warning',
            response.failedDocuments as ErrorResponse[]
          );
        }

        ctx.patchState({
          loading: false,
        });
        // load documents only for document root upload
        ctx.dispatch(new LoadDocuments(folderpath, true));
      }),
      catchError((err) => {
        if (err.error) {
          this.handleError('Warning', [err.error] as ErrorResponse[]);
        }
        ctx.patchState({ error: err });
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }
  @Action(UpdateDocument)
  UpdateDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: UpdateDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService
      .updateDocument(action.documentId, action.file)
      .pipe(
        tap((response: TDocument) => {
          ctx.dispatch(new CheckInDocument(response._id));
          ctx.patchState({
            loading: false,
            selectedDocument: response,
          });
          ctx.patchState({
            loading: false,
            folderPath: new FolderPath(response.path_url, [
              ...response.path_url.split('/'),
            ]),
          });
        }),
        catchError((err) => {
          if (err.error) {
            this.handleError('Warning', [err.error] as ErrorResponse[]);
          }
          ctx.patchState({ error: err });
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }
  @Action(CreateFolder)
  CreateFolder(
    ctx: StateContext<DocumentsStateModel>,
    action: CreateFolder
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const folderpath = ctx.getState().folderPath;
    return this._documentService
      .createFolder(folderpath.folderPath, action.folderName)
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            loading: false,
          });
          ctx.dispatch(new LoadDocuments(folderpath.folderPath, true));
        }),
        catchError((err) => {
          const errorMessage = err.error as ErrorResponse;
          this.handleError('Warning', [errorMessage] as ErrorResponse[]);
          ctx.patchState({ error: err });
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }

  /////// S E A R C H
  @Action(SearchFoldersHierarchy)
  SearchFoldersHierarchy(
    ctx: StateContext<DocumentsStateModel>,
    action: SearchFoldersHierarchy
  ): Observable<any> {
    ctx.patchState({
      error: null,
      searchInProgress: true,
    });
    return this._documentService
      .searchFoldersHierarchy(action.queryFilter)
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            searchInProgress: false,
            folderSearch: response,
          });
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            {
              status: 0,
              message: 'Cannot fetch the data.',
              code: '',
            },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ searchInProgress: false });
        })
      );
  }

  @Action(SearchThroughDocuments)
  SearchThroughDocuments(
    ctx: StateContext<DocumentsStateModel>,
    action: SearchThroughDocuments
  ): Observable<any> {
    ctx.patchState({
      searchInProgress: true,
      error: null,
    });
    return this._documentService
      .searchThroughDocuments(action.queryFilter)
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            searchInProgress: false,
            documentSearch: response,
          });
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            {
              status: 0,
              message: 'Cannot fetch the data.',
              code: '',
            },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ searchInProgress: false });
        })
      );
  }

  // C H E C K   I N  -  C H E C K   O U T
  @Action(CheckInDocument)
  CheckInDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: CheckInDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService.checkInDocument(action.documentId).pipe(
      tap((response: any) => {
        ctx.patchState({
          loading: false,
          selectedDocument: response,
        });
      }),
      catchError((err) => {
        const errorMessage = err.error as ErrorResponse;
        this.handleError('Warning', [errorMessage] as ErrorResponse[]);

        ctx.patchState({ error: err });
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }
  @Action(CheckOutDocument)
  CheckOutDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: CheckOutDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService.checkOutDocument(action.documentId).pipe(
      tap((response: any) => {
        ctx.patchState({
          loading: false,
          selectedDocument: response,
        });
      }),
      catchError((err) => {
        ctx.patchState({ error: err });
        this.handleError('Something Bad Happened', [
          { status: 0, message: 'Please try again.', code: '' },
        ]);
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  // D E L E T E  -  F O L D E R - D O C U M E N T
  @Action(DeleteDocument)
  DeleteDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: DeleteDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService.deleteDocument(action.documentId).pipe(
      tap((_response: any) => {
        ctx.patchState({
          loading: false,
        });
      }),
      catchError((err) => {
        ctx.patchState({ error: err });
        this.handleError('Something Bad Happened', [
          { status: 0, message: 'Please try again.', code: '' },
        ]);
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }
  @Action(DeleteFolder)
  DeleteFolder(
    ctx: StateContext<DocumentsStateModel>,
    action: DeleteFolder
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService.deleteFolder(action.folderId).pipe(
      tap((_response: any) => {
        ctx.patchState({
          loading: false,
        });
      }),
      catchError((err) => {
        ctx.patchState({ error: err });
        this.handleError('Something Bad Happened', [
          { status: 0, message: 'Please try again.', code: '' },
        ]);
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  // D O W N L O A D   D O C U M E N T S
  @Action(DownloadDocument)
  DownloadDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: DownloadDocument
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService
      .downloadDocument(action.documentId, action.isPublic)
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            loading: false,
          });
          const selectedDocument = ctx.getState().selectedDocument;
          this._fileSaverService.save(
            response,
            selectedDocument?.document_name
          );
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            { status: 0, message: 'Please try again.', code: '' },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }

  // g e t  D O C U M E N T   T E X T   C O N T E N T
  @Action(GetDocumentTextContent)
  GetDocumentTextContent(
    ctx: StateContext<DocumentsStateModel>,
    action: GetDocumentTextContent
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    return this._documentService.getDocumentTextContent(action.documentId).pipe(
      tap((response: any) => {
        const selectedDocument = ctx.getState().selectedDocument;
        if (selectedDocument) {
          ctx.patchState({
            loading: false,
            selectedDocument: {
              ...selectedDocument,
              text_content: response.data,
            },
          });
        }
      }),
      catchError((err) => {
        ctx.patchState({ error: err });
        this.handleError('Something Bad Happened', [
          { status: 0, message: 'Please try again.', code: '' },
        ]);
        return throwError(err);
      }),
      finalize(() => {
        ctx.patchState({ loading: false });
      })
    );
  }

  // P E R M I S S I O N S   A C T I O N S
  @Action(GetFolderPathPermissions)
  GetFolderPathPermissions(
    ctx: StateContext<DocumentsStateModel>,
    action: GetFolderPathPermissions
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentFolder = ctx.getState().rootFolder?.currentFolder || null;
    return this._documentService
      .getFolderPathPermissions(
        action.folderPath,
        currentFolder?._id || '',
        currentFolder?.parent_folder_id || ''
      )
      .pipe(
        tap((response: UserPermissionType[]) => {
          ctx.patchState({
            loading: false,
            folderPathPermissions: response,
          });
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('User Permissions', [
            {
              status: 0,
              message: 'Please check your access permissions.',
              code: '',
            },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }
  @Action(AddFolderPathPermissions)
  AddFolderPathPermissions(
    ctx: StateContext<DocumentsStateModel>,
    action: AddFolderPathPermissions
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentFolder = ctx.getState().rootFolder?.currentFolder || null;
    return this._documentService
      .addFolderPathPermissions(
        action.folderPath,
        currentFolder?._id || '',
        currentFolder?.parent_folder_id || '',
        action.permissionObj
      )
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            loading: false,
          });
          return response;
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            { status: 0, message: 'Please try again.', code: '' },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }
  @Action(UpdateUserPermissionOnFolderPath)
  UpdateUserPermissionOnFolderPath(
    ctx: StateContext<DocumentsStateModel>,
    action: UpdateUserPermissionOnFolderPath
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentFolder = ctx.getState().rootFolder?.currentFolder || null;
    return this._documentService
      .updateUserPermissionOnFolderPath(
        action.folderPath,
        currentFolder?._id || '',
        currentFolder?.parent_folder_id || '',
        action.permissionObj
      )
      .pipe(
        tap((_response: any) => {
          ctx.patchState({
            loading: false,
          });
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            { status: 0, message: 'Please try again.', code: '' },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }
  @Action(SetPathAccessType)
  SetPathAccessType(
    ctx: StateContext<DocumentsStateModel>,
    action: SetPathAccessType
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentFolder = ctx.getState().rootFolder?.currentFolder || null;
    return this._documentService
      .setPathAccessType(
        action.folderPath,
        currentFolder?._id || '',
        currentFolder?.parent_folder_id || currentFolder?.folder_id || '',
        action.accessTypeObj
      )
      .pipe(
        tap((response: any) => {
          ctx.patchState({
            loading: false,
            rootFolder: {
              ...ctx.getState().rootFolder,
              accessType: action.accessTypeObj.accessType,
            } as RDocuments,
          });
          return response;
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            { status: 0, message: 'Please try again.', code: '' },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }
  @Action(DeleteFolderPathPermissions)
  DeleteFolderPathPermissions(
    ctx: StateContext<DocumentsStateModel>,
    action: DeleteFolderPathPermissions
  ): Observable<any> {
    ctx.patchState({
      loading: true,
      error: null,
    });
    const currentFolder = ctx.getState().rootFolder?.currentFolder || null;
    return this._documentService
      .deleteFolderPathPermission(
        action.folderPath,
        currentFolder?._id || '',
        currentFolder?.parent_folder_id || '',
        action.userId
      )
      .pipe(
        tap((_response: any) => {
          ctx.patchState({
            loading: false,
          });
        }),
        catchError((err) => {
          ctx.patchState({ error: err });
          this.handleError('Something Bad Happened', [
            { status: 0, message: 'Please try again.', code: '' },
          ]);
          return throwError(err);
        }),
        finalize(() => {
          ctx.patchState({ loading: false });
        })
      );
  }

  // H E L P E R    A C T I O N S
  @Action(SetFolderOptions)
  SetFolderOptions(
    ctx: StateContext<DocumentsStateModel>,
    action: SetFolderOptions
  ): void {
    const currentFolderOptions = ctx.getState().options;
    const newFolderOptions = {
      ...currentFolderOptions,
      ...action.folderOptions,
    };
    this._storageService.setItem(
      StorageEnum.FOLDER_OTPIONS,
      JSON.stringify(newFolderOptions)
    );
    ctx.patchState({
      options: newFolderOptions,
    });
  }

  @Action(SelectDocument)
  async SelectDocument(
    ctx: StateContext<DocumentsStateModel>,
    action: SelectDocument
  ): Promise<void | Observable<any>> {
    ctx.patchState({
      selectedDocument: action.document,
    });
    if (
      action.document &&
      DocumentHelperClass.isTextFile(action.document.content_type)
    ) {
      ctx.dispatch(new GetDocumentTextContent(action.document._id));
    }
  }

  // p r i v a t e   m e t h o d s
  private handleError(title: string, errorMessages: ErrorResponse[]): void {
    this.dialog.closeAll();
    this.dialog.open(ErrorMessageComponent, {
      width: '50%',
      data: {
        title,
        data: errorMessages,
      },
    });
  }
}
