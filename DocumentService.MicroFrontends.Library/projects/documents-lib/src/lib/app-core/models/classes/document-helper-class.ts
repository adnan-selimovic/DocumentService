export class DocumentHelperClass {
  public static isTextFile(contentType: string): boolean {
    return contentType.includes('text');
  }

  public static getSize(
    bytes: number,
    decimals = 2
  ): { textValue: string; value: number } {
    if (bytes === 0) return { textValue: '0 Bytes', value: 0 };

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return {
      textValue:
        parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i],
      value: bytes,
    };
  }
}
