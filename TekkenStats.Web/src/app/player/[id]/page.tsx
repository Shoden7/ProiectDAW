'use client'
import { useParams } from 'next/navigation'
import { useEffect, useState } from 'react'
import { api } from '@/services/api'
import { useAuth } from '@/context/auth-context'
import { getRankColor, formatWinRate, CHARACTER_COLORS } from '@/utils/utils'
import type { PlayerDetail, Match, BookmarkDto } from '@/types'
import Navbar from '@/components/Navbar'
import Link from 'next/dist/client/link'

export default function PlayerProfilePage() {
  const params = useParams()
  const playerId = Number(params.id)

  const { isLoggedIn } = useAuth()
  const [player, setPlayer] = useState<PlayerDetail | null>(null)
  const [matches, setMatches] = useState<Match[]>([])
  const [bookmarks, setBookmarks] = useState<BookmarkDto[]>([])
  const [loading, setLoading] = useState(true)
  const [isBookmarked, setIsBookmarked] = useState(false)

  useEffect(() => {
    if (!playerId) return

    Promise.all([
      api.players.getById(playerId),
      api.players.getMatches(playerId, 1, 10),
      isLoggedIn ? api.bookmarks.list() : Promise.resolve([])
    ])
      .then(([playerData, matchData, bookmarkData]) => {
        setPlayer(playerData)
        setMatches(matchData.items)
        setBookmarks(bookmarkData)
        setIsBookmarked(bookmarkData.some(b => b.playerId === playerId))
      })
      .catch((err) => console.error("Data ingestion failed", err))
      .finally(() => setLoading(false))
  }, [playerId, isLoggedIn])

  const toggleBookmark = async () => {
    if (!player) return
    try {
      if (isBookmarked) {
        await api.bookmarks.remove(player.id)
        setIsBookmarked(false)
      } else {
        await api.bookmarks.add(player.id)
        setIsBookmarked(true)
      }
    } catch (err) {
      console.error("Bookmark shift operation rejected", err)
    }
  }

  if (loading) {
    return (
      <>
        <Navbar />
        <div className="flex-1 flex items-center justify-center font-mono text-xs text-iron-400">
          Syncing combat diagnostic matrices...
        </div>
      </>
    )
  }

  if (!player) {
    return (
      <>
        <Navbar />
        <div className="flex-1 flex items-center justify-center font-mono text-xs text-blood">
          CRITICAL ERROR: Player reference identity missing inside Database.
        </div>
      </>
    )
  }

  const rankColor = getRankColor(player.currentRank)

  return (
    <>
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-8 w-full space-y-8 animate-fade-in">
        
        {/* Profile Card Header */}
        <section className="bg-iron-900 border border-iron-800 p-6 md:p-8 rounded-sm relative shadow-2xl flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="absolute left-0 top-0 h-full w-1.5" style={{ backgroundColor: rankColor }} />
          
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <h1 className="font-display text-4xl uppercase tracking-wide text-white">{player.playerName}</h1>
              {isLoggedIn && (
                <button 
                  onClick={toggleBookmark}
                  className={`px-2 py-1 rounded-sm border font-mono text-xs transition-colors ${
                    isBookmarked 
                      ? 'bg-gold/10 border-gold text-gold' 
                      : 'border-iron-700 text-iron-400 hover:border-iron-500'
                  }`}
                >
                  {isBookmarked ? '★ Bookmarked' : '☆ Bookmark'}
                </button>
              )}
            </div>
            <p className="font-mono text-xs text-iron-400">Polaris Identification: {player.polarisId}</p>
            <div className="text-xs text-iron-500">Last Database Sync: {new Date(player.lastUpdated).toLocaleString()}</div>
          </div>

          {/* Quick Stats Grid */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 bg-iron-950 p-4 border border-iron-800 rounded-sm">
            <div className="text-center px-2">
              <div className="text-xs font-mono text-iron-400 uppercase">Rank Class</div>
              <div className="font-bold text-sm mt-1" style={{ color: rankColor }}>{player.currentRank}</div>
            </div>
            <div className="text-center px-2 border-l border-iron-800">
              <div className="text-xs font-mono text-iron-400 uppercase">Wins/Losses</div>
              <div className="font-bold text-sm mt-1 text-white">{player.wins}W / {player.losses}L</div>
            </div>
            <div className="text-center px-2 border-l border-iron-800">
              <div className="text-xs font-mono text-iron-400 uppercase">Win Ratio</div>
              <div className="font-bold text-sm mt-1 text-emerald-400">{formatWinRate(player.winRate)}</div>
            </div>
            <div className="text-center px-2 border-l border-iron-800">
              <div className="text-xs font-mono text-iron-400 uppercase">Main Specialty</div>
              {/* 🆕 Clickable Main Character tag */}
              <div className="font-bold text-sm mt-1">
                {player.mainCharacter ? (
                  <Link 
                    href={`/characters?char=${encodeURIComponent(player.mainCharacter)}`}
                    className="text-gold hover:text-gold-light hover:underline transition-colors"
                  >
                    {player.mainCharacter}
                  </Link>
                ) : (
                  <span className="text-iron-600">N/A</span>
                )}
              </div>
            </div>
          </div>
        </section>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          
          {/* Left Column: Character Stats breakdown */}
          <div className="space-y-4">
            <h3 className="font-display text-xl uppercase tracking-wider text-iron-300">Character Performance Layouts</h3>
            <div className="space-y-3">
              {player.characterStats?.map((c) => {
                const bgCharColor = CHARACTER_COLORS[c.characterName] || '#1a1a1f'
                return (
                  <div key={c.characterId} className="bg-iron-900 border border-iron-800 p-4 rounded-sm relative overflow-hidden">
                    <div className="absolute bottom-0 left-0 h-1 bg-emerald-500 transition-all" style={{ width: `${c.winRate * 100}%` }} />
                    
                    <div className="flex justify-between items-start relative z-10">
                      <div>
                        {/* 🆕 Character rows are now link headings */}
                        <Link 
                          href={`/characters?char=${encodeURIComponent(c.characterName)}`}
                          className="font-bold text-white hover:text-gold-light hover:underline transition-all flex items-center gap-2 group"
                        >
                          <span className="w-2 h-2 rounded-full" style={{ backgroundColor: bgCharColor }} />
                          {c.characterName}
                        </Link>
                        <div className="text-xs text-iron-400 font-mono mt-1">{c.wins} Wins / {c.losses} Losses</div>
                      </div>
                      <div className="text-right">
                        <span className="text-xs px-2 py-0.5 rounded-sm font-mono border" 
                              style={{ color: getRankColor(c.rank), borderColor: `${getRankColor(c.rank)}40`, backgroundColor: `${getRankColor(c.rank)}10` }}>
                          {c.rank}
                        </span>
                        <div className="text-xs font-semibold font-mono text-emerald-400 mt-1">{formatWinRate(c.winRate)}</div>
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Right Column: Historic Matches Log */}
          <div className="lg:col-span-2 space-y-4">
            <h3 className="font-display text-xl uppercase tracking-wider text-iron-300">Recent Combat Operations Logs</h3>
            <div className="bg-iron-900 border border-iron-800 rounded-sm divide-y divide-iron-800/60 overflow-hidden shadow-xl">
              {matches.length === 0 ? (
                <div className="p-8 text-center text-xs font-mono text-iron-500">No match matrices recorded for this sector profile.</div>
              ) : (
                matches.map((match) => (
                  <div key={match.id} className={`p-4 flex flex-col md:flex-row md:items-center justify-between gap-4 transition-colors ${match.won ? 'bg-emerald-500/5' : 'bg-blood/5'}`}>
                    <div className="flex items-center gap-4">
                      <div className={`w-12 h-12 flex items-center justify-center font-mono text-xs font-bold border rounded-sm ${
                        match.won 
                          ? 'border-emerald-500/40 bg-emerald-500/10 text-emerald-400' 
                          : 'border-blood/40 bg-blood/10 text-blood-light'
                      }`}>
                        {match.won ? 'WIN' : 'LOSS'}
                      </div>
                      
                      <div>
                        <div className="text-sm font-bold text-white flex flex-wrap items-center gap-x-1.5">
                          {/* 🆕 1. Clickable Link for Your Character */}
                          <Link 
                            href={`/characters?char=${encodeURIComponent(match.myCharacter)}`}
                            className="text-gold-light hover:underline hover:text-gold transition-colors"
                          >
                            {match.myCharacter}
                          </Link>
                          
                          <span className="text-xs text-iron-500 font-normal">vs</span> 
                          
                          {/* 2. Clickable Link for Opponent Name */}
                          <Link 
                            href={`/player/${match.opponentId}`}
                            className="text-blood-glow hover:underline hover:text-white transition-colors cursor-pointer"
                          >
                            {match.opponentName}
                          </Link>
                          
                          {/* 🆕 3. Clickable Link for Opponent Character selection */}
                          <span className="text-xs text-iron-400 font-normal flex gap-1">
                            (
                            <Link 
                              href={`/characters?char=${encodeURIComponent(match.opponentCharacter)}`}
                              className="text-iron-300 hover:underline hover:text-white transition-colors"
                            >
                              {match.opponentCharacter}
                            </Link>
                            )
                          </span>
                        </div>
                        <div className="text-xs text-iron-400 font-mono mt-0.5">
                          Region Grid: {match.region || 'Global'} • Rounds: {match.rounds}
                        </div>
                      </div>
                    </div>

                    <div className="text-right font-mono text-xs">
                      <div className="text-iron-300">Dan Skill Threshold: {match.myRankDan}</div>
                      <div className="text-iron-500 mt-1">{new Date(match.foughtAt).toLocaleDateString()}</div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>

        </div>
      </main>
    </>
  )
}