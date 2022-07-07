import { HttpParams } from '@angular/common/http';

export class QueryFilter {
  page = 1;
  pageSize = 50;
  searchTerm = '';

  increasePage() {
    this.page = this.page + 1;
  }
  decreasePage() {
    this.page = this.page - 1;
  }

  getQueryString() {
    const httpParams = new HttpParams({
      fromObject: {
        page: this.page,
        pageSize: this.pageSize,
        searchTerm: this.searchTerm,
      },
    });
    return `?${httpParams.toString()}`;
  }
}
