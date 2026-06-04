'use client'
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/context/auth-context'
import { api } from '@/services/api'
import Link from 'next/link'
import Navbar from '@/components/Navbar'

export default function RegisterPage() {
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const { login } = useAuth()
  const router = useRouter()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const data = await api.auth.register(username, email, password)
      login(data)
      router.push('/')
    } catch (err: any) {
      setError(err.message || 'Registration dropped. Name coordinates or Email duplicated.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      <Navbar />
      <div className="flex-1 flex items-center justify-center px-4 py-12 animate-slide-up">
        <div className="w-full max-w-md bg-iron-900 border border-iron-800 p-8 rounded-sm shadow-2xl relative">
          <div className="absolute top-0 left-0 w-1 h-full bg-gold" />
          
          <h2 className="font-display text-3xl tracking-wide text-white uppercase mb-2">Enlist Profile</h2>
          <p className="text-xs text-iron-400 font-mono uppercase mb-6">Initialize a new global tracking dataset</p>

          {error && (
            <div className="mb-4 p-3 bg-blood/10 border border-blood/30 text-blood-light text-xs font-mono rounded-sm">
              ⚠️ REGISTRATION FAULT: {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4 font-body">
            <div>
              <label className="block text-xs font-mono uppercase text-iron-400 mb-1">Fighter Alias (Username)</label>
              <input
                type="text"
                required
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full bg-iron-950 border border-iron-700 focus:border-gold px-3 py-2 text-white outline-none transition-colors text-sm rounded-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-mono uppercase text-iron-400 mb-1">Email Vector</label>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-iron-950 border border-iron-700 focus:border-gold px-3 py-2 text-white outline-none transition-colors text-sm rounded-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-mono uppercase text-iron-400 mb-1">Cipher Key (Password)</label>
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-iron-950 border border-iron-700 focus:border-gold px-3 py-2 text-white outline-none transition-colors text-sm rounded-sm"
              />
            </div>
            <button type="submit" disabled={loading} className="w-full btn-primary !bg-gold hover:!bg-gold-light text-iron-950 font-bold mt-6">
              {loading ? 'Generating Hash File...' : 'Commit Database Record'}
            </button>
          </form>

          <p className="mt-6 text-center text-xs text-iron-400 font-mono">
            Already verified? <Link href="/login" className="text-gold-light hover:underline">Access terminal login →</Link>
          </p>
        </div>
      </div>
    </>
  )
}