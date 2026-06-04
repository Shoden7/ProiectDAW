'use client'
import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import type { AuthResponse } from '@/types'

interface AuthCtx {
  user: { userId: number; username: string } | null
  token: string | null
  login: (data: AuthResponse) => void
  logout: () => void
  isLoggedIn: boolean
}

const Ctx = createContext<AuthCtx | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<{ userId: number; username: string } | null>(null)
  const [token, setToken] = useState<string | null>(null)

  useEffect(() => {
    const t = localStorage.getItem('token')
    const u = localStorage.getItem('user')
    if (t && u) { setToken(t); setUser(JSON.parse(u)) }
  }, [])

  const login = (data: AuthResponse) => {
    localStorage.setItem('token', data.token)
    const u = { userId: data.userId, username: data.username }
    localStorage.setItem('user', JSON.stringify(u))
    setToken(data.token)
    setUser(u)
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setToken(null)
    setUser(null)
  }

  return (
    <Ctx.Provider value={{ user, token, login, logout, isLoggedIn: !!token }}>
      {children}
    </Ctx.Provider>
  )
}

export const useAuth = () => {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useAuth must be inside AuthProvider')
  return ctx
}
