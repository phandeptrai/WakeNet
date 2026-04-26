import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../services/api'

export default function HomePage() {
  const nav = useNavigate()
  const [me, setMe] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    ;(async () => {
      try {
        const data = await api('/api/auth/me')
        console.info('[Auth] Session hợp lệ (me):', data)
        setMe(data)
      } catch (err) {
        console.warn('[Auth] Chưa đăng nhập / session không hợp lệ:', err)
        setMe(null)
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  async function logout() {
    console.info('[Auth] Logout...')
    await api('/api/auth/logout', { method: 'POST' })
    console.info('[Auth] Logout thành công')
    nav('/login')
  }

  if (loading) return <div className="container">Loading...</div>

  if (!me)
    return (
      <div className="container">
        <h1>WakeNet</h1>
        <p>You are not logged in.</p>
        <Link to="/login">Go to login</Link>
      </div>
    )

  return (
    <div className="container">
      <h1>WakeNet</h1>
      <p>
        Logged in as <b>{me.username}</b>
      </p>
      <button onClick={logout}>Logout</button>
    </div>
  )
}

