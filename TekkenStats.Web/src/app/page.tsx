'use client'
import { useState, useEffect } from 'react'
import { api } from '@/services/api'
import { getRankColor, formatWinRate } from '@/utils/utils'
import type { LeaderboardEntry, PlayerSearchResult } from '@/types'
import Navbar from '@/components/Navbar'
import Link from 'next/link'

export default function HomePage() {
  const [searchQuery, setSearchQuery] = useState('')
  const [searchResults, setSearchResults] = useState<PlayerSearchResult[]>([])
  const [leaderboard, setLeaderboard] = useState<LeaderboardEntry[]>([])
  
  // 🆕 Pagination tracking state properties
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const pageSize = 20 // Rows per page view layer

  const [loadingSearch, setLoadingSearch] = useState(false)
  const [loadingLeaderboard, setLoadingLeaderboard] = useState(true)

  // 🆕 Re-poll leaderboard data anytime currentPage modifications fire
  useEffect(() => {
    setLoadingLeaderboard(true)
    api.players.leaderboard(currentPage, pageSize)
      .then((res) => {
        setLeaderboard(res.items)
        // Calculate dynamic ceiling limit
        setTotalPages(Math.ceil(res.totalCount / res.pageSize) || 1)
      })
      .catch((err) => console.error("Failed loading leaderboard matrix metrics", err))
      .finally(() => setLoadingLeaderboard(false))
  }, [currentPage])

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!searchQuery.trim()) return
    setLoadingSearch(true)
    try {
      const res = await api.players.search(searchQuery)
      setSearchResults(res.items)
    } catch (err) {
      console.error("Search index configuration dropped", err)
    } finally {
      setLoadingSearch(false)
    }
  }

  return (
    <>
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-8 flex-1 w-full animate-fade-in">
        
        {/* Search Section Layout */}
        <section className="text-center my-12 max-w-2xl mx-auto">
          <h1 className="font-display text-5xl md:text-6xl tracking-tight mb-4 uppercase">
            Track Your <span className="text-gradient-red">Tekken 8</span> Legacy
          </h1>
          <p className="text-iron-400 font-body mb-8">
            Analyze match data, ranked histories, and character diagnostics pulled directly via Wavu metrics.
          </p>

          <form onSubmit={handleSearch} className="flex gap-2 p-1 bg-iron-900 border border-iron-700 focus-within:border-blood transition-colors rounded-sm">
            <input
              type="text"
              placeholder="Search Player Name (e.g., Knee, Arslan Ash)..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="bg-transparent flex-1 px-4 py-3 outline-none font-body text-white"
            />
            <button type="submit" className="btn-primary">
              {loadingSearch ? 'Searching...' : 'Search'}
            </button>
          </form>

          {/* Search Results Display Area */}
          {searchResults.length > 0 && (
            <div className="mt-4 bg-iron-900 border border-iron-700 text-left rounded-sm divide-y divide-iron-800 shadow-xl max-h-60 overflow-y-auto">
              {searchResults.map((player) => (
                <div key={player.id} className="p-4 flex justify-between items-center hover:bg-iron-800/40 transition-colors group">
                  <div>
                    <Link href={`/player/${player.id}`} className="font-semibold text-white hover:text-blood-glow transition-colors block">
                      {player.playerName}
                    </Link>
                    <div className="text-xs font-mono text-iron-500">Polaris: {player.polarisId}</div>
                  </div>
                  <div className="flex items-center gap-3">
                    {/* 🆕 Clickable character tag linkage to analytics hub */}
                    <Link 
                      href={`/characters?char=${encodeURIComponent(player.mainCharacter)}`}
                      className="text-xs bg-iron-950 border border-iron-800 px-2 py-1 text-gold-light hover:border-gold transition-colors"
                    >
                      {player.mainCharacter}
                    </Link>
                    <span 
                      className="rank-badge rounded-sm"
                      style={{ backgroundColor: `${getRankColor(player.currentRank)}20`, color: getRankColor(player.currentRank), border: `1px solid ${getRankColor(player.currentRank)}40` }}
                    >
                      {player.currentRank}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>

        {/* Global Leaderboard Table */}
        <section className="mt-16 space-y-6">
          <div>
            <h2 className="font-display text-3xl uppercase tracking-wider">Global Leaderboard</h2>
            <p className="text-xs text-iron-400 font-mono">TOP PLAYERS RANKED BY TEKKEN PERFORMANCE</p>
          </div>

          <div className="bg-iron-900 border border-iron-800 rounded-sm overflow-x-auto shadow-2xl">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-iron-800 text-iron-400 text-xs font-mono uppercase bg-iron-950">
                  <th className="p-4 w-16 text-center">Rank</th>
                  <th className="p-4">Fighter</th>
                  <th className="p-4">Main Specialty</th>
                  <th className="p-4">Rank Tier</th>
                  <th className="p-4 text-center">Wins</th>
                  <th className="p-4 text-center">Win Rate</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-iron-800/60 font-body text-sm">
                {loadingLeaderboard ? (
                  [...Array(10)].map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      <td colSpan={6} className="p-6 text-center text-iron-500 font-mono">Loading data matrix row parameters...</td>
                    </tr>
                  ))
                ) : (
                  leaderboard.map((entry) => (
                    <tr key={entry.id} className="hover:bg-iron-800/40 transition-colors">
                      <td className="p-4 text-center font-mono font-bold text-iron-400">
                        {((currentPage - 1) * pageSize) + entry.rank}
                      </td>
                      <td className="p-4">
                        <Link href={`/player/${entry.id}`} className="font-semibold text-white hover:text-blood-glow transition-colors">
                          {entry.playerName}
                        </Link>
                        <div className="text-xs text-iron-500 font-mono block">{entry.polarisId}</div>
                      </td>
                      <td className="p-4">
                        {/* 🆕 Clickable character navigation route link block */}
                        {entry.mainCharacter ? (
                          <Link 
                            href={`/characters?char=${encodeURIComponent(entry.mainCharacter)}`}
                            className="text-xs bg-iron-950 px-2 py-1 border border-iron-800 hover:border-gold-light text-iron-200 transition-colors inline-block"
                          >
                            👤 {entry.mainCharacter}
                          </Link>
                        ) : (
                          <span className="text-xs text-iron-600 font-mono">Unset</span>
                        )}
                      </td>
                      <td className="p-4">
                        <span 
                          className="rank-badge rounded-sm px-2.5 py-1"
                          style={{ 
                            backgroundColor: `${getRankColor(entry.currentRank)}15`, 
                            color: getRankColor(entry.currentRank),
                            border: `1px solid ${getRankColor(entry.currentRank)}30` 
                          }}
                        >
                          {entry.currentRank}
                        </span>
                      </td>
                      <td className="p-4 text-center font-mono text-iron-300">{entry.wins}</td>
                      <td className="p-4 text-center font-mono font-semibold text-emerald-400 bg-emerald-500/5">
                        {formatWinRate(entry.winRate)}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* 🆕 Pagination Dashboard Controls Interface */}
          <div className="flex justify-between items-center bg-iron-900 border border-iron-800 p-4 rounded-sm font-mono text-xs">
            <span className="text-iron-400">
              Page <span className="text-white font-bold">{currentPage}</span> of <span className="text-white">{totalPages}</span>
            </span>
            <div className="flex gap-2">
              <button 
                onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                disabled={currentPage === 1 || loadingLeaderboard}
                className="btn-ghost !py-1 !px-3 disabled:opacity-30 disabled:hover:border-iron-600 cursor-pointer"
              >
                ◀ Previous
              </button>
              <button 
                onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                disabled={currentPage === totalPages || loadingLeaderboard}
                className="btn-ghost !py-1 !px-3 disabled:opacity-30 disabled:hover:border-iron-600 cursor-pointer"
              >
                Next ▶
              </button>
            </div>
          </div>
        </section>

      </main>
    </>
  )
}