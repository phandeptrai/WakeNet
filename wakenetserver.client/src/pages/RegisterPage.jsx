import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../services/api'

export default function RegisterPage() {
  const nav = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const me = await api('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, password }),
      })
      console.info('[Auth] Register thành công:', me)
      nav('/')
    } catch (err) {
      console.error('[Auth] Register thất bại:', err)
      setError(err.message || 'Register failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <h1>Register</h1>
      <form onSubmit={onSubmit} className="card">
        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </label>
        {error ? <div className="error">{error}</div> : null}
        <button disabled={loading} type="submit">
          {loading ? '...' : 'Create account'}
        </button>
        <div className="muted">
          Have an account? <Link to="/login">Login</Link>
        </div>
      </form>
    </div>
  )
}

