import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }
  getItem<T>(key: string): T | null {
    const value = localStorage.getItem(key);
    return value ? (JSON.parse(value) as T) : null;
  }

  clear(): void {
    localStorage.clear();
  }
}
