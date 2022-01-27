// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { default as styles } from './App.module.scss';
import { Navigation } from './Navigation';
import { Home } from './Home';
import { Applications } from './Applications/Applications';

export const App = () => {
    return (
        <Router>
            <div className={styles.appContainer}>
                <div className={styles.navigationBar}>
                    <Navigation />
                </div>
                <div style={{ width: '100%' }}>
                    <Routes>
                        <Route path="/" element={<Home/>}/>
                        <Route path="/applications" element={<Applications/>}/>
                    </Routes>
                </div>
            </div>
        </Router>
    );
};
