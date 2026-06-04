export interface PlayerSummary {
  id: number
  polarisId: string
  playerName: string
  currentRank: string
  danRank: number
  wins: number
  losses: number
  mainCharacter: string
  winRate: number
  lastUpdated: string
}

export interface CharacterStats {
  characterName: string
  characterId: number
  wins: number
  losses: number
  rank: string
  danRank: number
  winRate: number
}

export interface PlayerDetail extends PlayerSummary {
  characterStats: CharacterStats[]
}

export interface Match {
  id: number
  battleId: string
  opponentId: number
  opponentName: string
  myCharacter: string
  opponentCharacter: string
  myRankDan: number
  opponentRankDan: number
  won: boolean
  rounds: number
  foughtAt: string
  region: string
}

export interface LeaderboardEntry {
  rank: number
  id: number
  polarisId: string
  playerName: string
  currentRank: string
  danRank: number
  wins: number
  losses: number
  winRate: number
  mainCharacter: string
}

export interface PlayerSearchResult {
  id: number
  polarisId: string
  playerName: string
  currentRank: string
  danRank: number
  mainCharacter: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface AuthResponse {
  token: string
  username: string
  userId: number
  expiresAt: string
}

export interface BookmarkDto {
  id: number
  playerId: number
  playerName: string
  polarisId: string
  currentRank: string
  mainCharacter: string
  bookmarkedAt: string
}


export interface CharacterGlobalStats {
  characterName: string
  globalWinRate: number
  totalMatchesPlayed: number
  popularityRate: number
}

export interface CharacterMatchupRow {
  characterName: string
  vsCharacterName: string
  winRateRatio: number // e.g., 0.545 for 54.5% winrate
  totalGames: number
}

export interface CharacterMatrixResponse {
  characterName: string
  overallWinRate: number
  topPlayers: PlayerSearchResult[]
  matchups: CharacterMatchupRow[]
}