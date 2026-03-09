const { Sequelize, DataTypes } = require('sequelize');

const sequelize = new Sequelize(
    process.env.MYSQL_DATABASE,
    process.env.MYSQL_USER,
    process.env.MYSQL_PASSWORD,
    {
        host: process.env.MYSQL_HOST,
        port: process.env.MYSQL_PORT,
        dialect: 'mysql',
        logging: true,
        define: {
            timestamps: false,
        }
    }
)


const User = sequelize.define('User', {
    userId: {
        type: DataTypes.UUID,
        defaultValue: DataTypes.UUIDV4,
        primaryKey: true
    },
    name: {
        type: DataTypes.STRING,
        allowNull: false
    },
    password: {
        type: DataTypes.STRING,
        allowNull: false
    },
})

const Score = sequelize.define('Score', {
    scoreId: {
        type: DataTypes.UUID,
        defaultValue: DataTypes.UUIDV4,
        primaryKey: true
    },
    pointTotal: {
        type: DataTypes.INTEGER,
        allowNull: false
    },
    rank: {
        type: DataTypes.INTEGER,
        allowNull: false
    }
})

const Level = sequelize.define('Level', {
    levelId: {
        type: DataTypes.UUID,
        defaultValue: DataTypes.UUIDV4,
        primaryKey: true
    },
    name: {
        type: DataTypes.STRING,
        allowNull: false
    }
})

User.hasMany(Score, { foreignKey: 'userId', onDelete: 'SET NULL'});
Score.belongsTo(User, { foreignKey: 'userId' });
Level.hasMany(Score, { foreignKey: 'levelId' , onDelete: 'SET NULL' });
Score.belongsTo(Level, { foreignKey: 'levelId' });

module.exports = {
    sequelize,
    User,
    Score,
    Level
}