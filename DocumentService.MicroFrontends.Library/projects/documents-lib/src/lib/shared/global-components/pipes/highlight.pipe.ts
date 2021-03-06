import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'highlightPipe',
})
export class HighlightPipe implements PipeTransform {
  transform(text: string, filter: string): any {
    if (filter) {
      text = text.replace(
        new RegExp('(' + filter + ')', 'gi'),
        '<span><b>$1</b></span>'
      );
    }
    return text;
  }
}
