import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

// One pick on the slip. We snapshot the odd at click-time so the slip
// stays stable even if the match's odds get updated later.
export interface BetSelection {
  matchId: number;
  homeTeam: string;
  awayTeam: string;
  outcome: '1' | 'X' | '2';
  odd: number;
}

@Injectable({
  providedIn: 'root'
})
export class BetSlipService {
  private apiUrl = 'http://localhost:5257/api/bets';

  // The list of current selections. Components subscribe to this.
  private selectionsSubject = new BehaviorSubject<BetSelection[]>([]);
  public selections$ = this.selectionsSubject.asObservable();

  // The stake the user typed. Kept here so it survives across components.
  private stakeSubject = new BehaviorSubject<number>(0);
  public stake$ = this.stakeSubject.asObservable();

  constructor(private http: HttpClient) {}

  // --- reading current values synchronously when we need them ---
  private get selections(): BetSelection[] {
    return this.selectionsSubject.value;
  }

  // Same source HotbarService reads from. Kept local so the slip doesn't
  // depend on HotbarService just to know who's logged in.
  private getUsername(): string {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      return user.username || user.Username || '';
    }
    return '';
  }

  // Add or replace a selection. One pick per match: if this match is
  // already on the slip, swap the outcome instead of adding a duplicate.
  addSelection(selection: BetSelection): void {
    const existing = this.selections.filter(s => s.matchId !== selection.matchId);
    this.selectionsSubject.next([...existing, selection]);
  }

  removeSelection(matchId: number): void {
    this.selectionsSubject.next(
      this.selections.filter(s => s.matchId !== matchId)
    );
  }

  // Used by the matches component to highlight which odd is currently picked.
  getSelectedOutcome(matchId: number): '1' | 'X' | '2' | null {
    const found = this.selections.find(s => s.matchId === matchId);
    return found ? found.outcome : null;
  }

  setStake(amount: number): void {
    // Guard against NaN/negative; floor at 0.
    this.stakeSubject.next(amount > 0 ? amount : 0);
  }

  clear(): void {
    this.selectionsSubject.next([]);
    this.stakeSubject.next(0);
  }

  // --- derived values ---

  // Accumulator odds = product of all selection odds.
  getTotalOdds(): number {
    if (this.selections.length === 0) return 0;
    return this.selections.reduce((acc, s) => acc * s.odd, 1);
  }

  // Payout = stake * total odds.
  getPotentialPayout(): number {
    return this.stakeSubject.value * this.getTotalOdds();
  }

  getCount(): number {
    return this.selections.length;
  }

  // Sends only matchId + outcome per selection. No odds, no payout —
  // the server computes those. Returns the backend response observable.
  placeBet(stake: number): Observable<any> {
    const username = this.getUsername();
    const payload = {
      username: username,
      stake: stake,
      selections: this.selections.map(s => ({
        matchId: s.matchId,
        outcome: s.outcome
      }))
    };
    return this.http.post(`${this.apiUrl}`, payload);
  }
}