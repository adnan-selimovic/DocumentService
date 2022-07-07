export class DateHelpers {
  public static datesDiffInMinutes(date1: any, date2: any): number {
    return Math.floor(
      Math.abs(new Date(date1).getTime() - new Date(date2).getTime()) /
        1000 /
        60
    );
  }
}
