export class FolderPath {
  folderPath!: string;
  folderPathArr!: string[];

  constructor(folderPath: string, folderPathArr: string[]) {
    this.folderPath = folderPath;
    this.folderPathArr = folderPathArr;
  }

  getPathOnIndex(index: number) {
    const url = this.folderPathArr.slice(0, index + 1).join('/');
    return url;
  }

  getCurrentFolder(): string | null {
    const length = this.folderPathArr ? this.folderPathArr.length : 0;
    if (length > 0) {
      return this.folderPathArr[length - 1];
    }
    return null;
  }
  getPreviousFolder(): string | null {
    const length = this.folderPathArr ? this.folderPathArr.length : 0;
    if (length > 1) {
      const url = this.folderPathArr.slice(0, length - 1).join('/');
      return url === '' ? '/' : url;
    }
    return null;
  }
}
