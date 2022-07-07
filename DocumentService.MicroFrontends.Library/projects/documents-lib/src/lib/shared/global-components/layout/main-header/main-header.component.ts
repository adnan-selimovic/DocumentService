import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-main-header',
  templateUrl: './main-header.component.html',
  styleUrls: ['./main-header.component.scss'],
})
export class MainHeaderComponent implements OnInit {
  IS_PUBLIC = false;
  form = this.fb.group({
    searchInput: [],
  });
  get searchInput() {
    return this.form.get('searchInput');
  }

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.searchInput?.valueChanges.subscribe((values) => {
      this.router.navigate(['/documents/search'], {
        queryParams: { text: values },
      });
    });

    if (this.router.url.includes('/documents/public')) {
      this.IS_PUBLIC = true;
    }

    this.route.queryParams.subscribe((qp: any) => {
      if (qp && qp.text) {
        this.searchInput?.setValue(qp.text);
      }
    });
  }
}
