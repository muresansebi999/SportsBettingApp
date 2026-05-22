export interface Match {
  id: number;
  apiId?: string;
  homeTeam: string;
  awayTeam: string;
  league: string;
  homeOdds: number;
  drawOdds: number;
  awayOdds: number;
  startTime: string;
  isFinished: boolean;
  homeScore?: number;
  awayScore?: number;
}