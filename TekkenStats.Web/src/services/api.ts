import type {
  AuthResponse, BookmarkDto, LeaderboardEntry,
  Match, PagedResult, PlayerDetail, PlayerSearchResult,
  CharacterGlobalStats, CharacterMatrixResponse
} from '@/types'

const BASE = '/api'

function getToken(): string | null {
  if (typeof window === 'undefined') return null
  return localStorage.getItem('token')
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken()
  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText)
    throw new Error(text || `HTTP ${res.status}`)
  }
  // 204 No Content
  if (res.status === 204) return undefined as T
  return res.json()
}

// Players and Systems endpoints
export const api = {
  players: {
    search: (name: string, page = 1, pageSize = 20) =>
      request<PagedResult<PlayerSearchResult>>(`/players/search?name=${encodeURIComponent(name)}&page=${page}&pageSize=${pageSize}`),
    getById: (id: number) =>
      request<PlayerDetail>(`/players/${id}`),
    getByPolarisId: (polarisId: string) =>
      request<PlayerDetail>(`/players/polaris/${polarisId}`),
    getMatches: (id: number, page = 1, pageSize = 20) =>
      request<PagedResult<Match>>(`/players/${id}/matches?page=${page}&pageSize=${pageSize}`),
    leaderboard: (page = 1, pageSize = 50) =>
      request<PagedResult<LeaderboardEntry>>(`/players/leaderboard?page=${page}&pageSize=${pageSize}`),
  },

  auth: {
    register: (username: string, email: string, password: string) =>
      request<AuthResponse>('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password }),
      }),
    login: (email: string, password: string) =>
      request<AuthResponse>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      }),
    linkPolaris: (polarisId: string) =>
      request<void>('/auth/link-polaris', {
        method: 'POST',
        body: JSON.stringify({ polarisId }),
      }),
  },

  bookmarks: {
    list: () => request<BookmarkDto[]>('/bookmarks'),
    add: (playerId: number) =>
      request<BookmarkDto>('/bookmarks', {
        method: 'POST',
        body: JSON.stringify({ playerId }),
      }),
    remove: (playerId: number) =>
      request<void>(`/bookmarks/${playerId}`, {
        method: 'DELETE',
      }),
  },

  characters: {
    getOverview: () => 
      request<CharacterGlobalStats[]>('/characters/overview'),
    getDetails: (characterName: string) => 
      request<CharacterMatrixResponse>(`/characters/${encodeURIComponent(characterName)}`),
  },
}