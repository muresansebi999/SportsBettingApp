import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, of } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HotbarService {
  private apiUrl = 'http://localhost:5257/api/hotbar';
  private authUrl = 'http://localhost:5257/api/auth';

  public onLoginSuccess = new Subject<void>();

  constructor(private http: HttpClient) { }

  private getUsername(): string {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      return user.username || user.Username || '';
    }
    return '';
  }

  getHotbarInfo(): Observable<any> {
    const username = this.getUsername();
    if (!username) return of({ username: '', balance: 0 }); 
    return this.http.get(`${this.apiUrl}/info/${username}`);
  }

  depositFunds(amount: number): Observable<any> {
    const username = this.getUsername();
    return this.http.post(`${this.apiUrl}/deposit`, { username: username, amount: amount });
  }

  withdrawFunds(amount: number): Observable<any> {
    const username = this.getUsername();
    return this.http.post(`${this.apiUrl}/withdraw`, { username: username, amount: amount });
  }

  updateProfile(oldUsername: string, newUsername: string, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/update`, {
      oldUsername: oldUsername,
      newUsername: newUsername,
      email: email
    });
  }

  deleteAccount(username: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/delete/${username}`, {});
  }

  login(model: any): Observable<any> {
    return this.http.post(`${this.authUrl}/login`, model).pipe(
      map((user: any) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.onLoginSuccess.next();
        }
        return user;
      })
    );
  }

  logout(): void {
    localStorage.removeItem('user');
    this.onLoginSuccess.next();
  }

  getCurrentUser(): any {
    const userString = localStorage.getItem('user');
    return userString ? JSON.parse(userString) : null;
  }
} 