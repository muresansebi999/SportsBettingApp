import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { BetSlipService, BetSelection } from '../../services/bet_slip_service';
import { HotbarService } from '../../services/hotbar';
import { BetHistoryService, BetHistoryItem } from '../../services/bet_history_service';

@Component({
  selector: 'app-bet-slip',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bet-slip.html',
  styleUrls: ['./bet-slip.css']
})
export class BetSlipComponent implements OnInit, OnDestroy {
  selections: BetSelection[] = [];
  stake: number | null = null;

  placing = false;
  errorMsg: string | null = null;
  successMsg: string | null = null;

  // Tab state: which of the three panels is showing.
  activeTab: 'bilet' | 'activ' | 'istoric' = 'bilet';
  allBets: BetHistoryItem[] = [];
  loadingBets = false;

  private subs: Subscription[] = [];

  constructor(
    private betSlip: BetSlipService,
    private hotbar: HotbarService,
    private betHistory: BetHistoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.subs.push(
      this.betSlip.selections$.subscribe(sel => {
        this.selections = sel;
        this.cdr.detectChanges();
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  // Called on every keystroke in the stake box.
  onStakeChange(): void {
    const value = this.stake ?? 0;
    this.betSlip.setStake(value);
  }

  remove(matchId: number): void {
    this.betSlip.removeSelection(matchId);
  }

  clearAll(): void {
    this.stake = null;
    this.betSlip.clear();
  }

  get totalOdds(): number {
    return this.betSlip.getTotalOdds();
  }

  get potentialPayout(): number {
    return this.betSlip.getPotentialPayout();
  }

  // Human label for the picked outcome.
  outcomeLabel(sel: BetSelection): string {
    if (sel.outcome === '1') return sel.homeTeam;
    if (sel.outcome === '2') return sel.awayTeam;
    return 'Egal'; // X = draw
  }

  // --- Tab switching + history fetching ---

  setTab(tab: 'bilet' | 'activ' | 'istoric'): void {
    this.activeTab = tab;
    // Re-fetch when entering a bet-list tab, so settlement done via Swagger
    // shows up without a page reload.
    if (tab === 'activ' || tab === 'istoric') {
      this.loadBets();
    }
  }

  loadBets(): void {
    this.loadingBets = true;
    this.betHistory.getMyBets().subscribe({
      next: (bets) => {
        this.allBets = bets;
        this.loadingBets = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingBets = false;
        this.cdr.detectChanges();
      }
    });
  }

  get activeBets(): BetHistoryItem[] {
    return this.allBets.filter(b => b.status === 'Pending');
  }

  get settledBets(): BetHistoryItem[] {
    return this.allBets.filter(b => b.status === 'Won' || b.status === 'Lost');
  }

  // "Câștigător" / "Necâștigător" for the istoric tab.
  resultLabel(status: string): string {
    return status === 'Won' ? 'Câștigător' : 'Necâștigător';
  }

  // Renders each selection's pick in the history cards.
  pickLabel(sel: { outcome: string; homeTeam: string; awayTeam: string }): string {
    if (sel.outcome === '1') return sel.homeTeam;
    if (sel.outcome === '2') return sel.awayTeam;
    return 'Egal';
  }

  placeBet(): void {
    this.errorMsg = null;
    this.successMsg = null;

    const stakeValue = this.stake ?? 0;
    if (this.selections.length === 0 || stakeValue <= 0) return;

    this.placing = true;
    this.betSlip.placeBet(stakeValue).subscribe({
      next: (res) => {
        this.placing = false;
        this.successMsg = `Bilet plasat! Câștig potențial: ${res.potentialPayout} $`;
        this.hotbar.onLoginSuccess.next(); // refresh displayed balance
        this.stake = null;
        this.betSlip.clear();
        this.loadBets(); // refresh so the new bet shows in "Bilete active"
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.placing = false;
        this.errorMsg = typeof err.error === 'string' ? err.error : 'Eroare la plasarea biletului.';
        this.cdr.detectChanges();
      }
    });
  }
}