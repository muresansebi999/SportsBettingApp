import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

// Mirrors what BetsController's GET /api/bets/my returns.
export interface BetHistorySelection {
  matchId: number;
  homeTeam: string;
  awayTeam: string;
  outcome: '1' | 'X' | '2';
  odd: number;
}

export interface BetHistoryItem {
  id: number;
  stake: number;
  totalOdds: number;
  potentialPayout: number;
  status: 'Pending' | 'Won' | 'Lost';
  createdAt: string;
  settledAt: string | null;
  selections: BetHistorySelection[];
}

@Injectable({
  providedIn: 'root'
})
export class BetHistoryService {
  private apiUrl = 'http://localhost:5257/api/bets';

  constructor(private http: HttpClient) {}

  private getUsername(): string {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      return user.username || user.Username || '';
    }
    return '';
  }

  getMyBets(): Observable<BetHistoryItem[]> {
    const username = this.getUsername();
    if (!username) return of([]);
    return this.http.get<BetHistoryItem[]>(`${this.apiUrl}/my?username=${username}`);
  }
}